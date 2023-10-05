from datetime import datetime
import json
import random
from django.views.decorators.csrf import csrf_exempt
from django.http import HttpResponse, JsonResponse
from django.shortcuts import render
import logging
import boto3
from botocore.exceptions import ClientError
import os
import requests
from django.contrib import messages
from django.core.files.storage import FileSystemStorage


class User:
    def __init__(self, firstname, lastname, phone, email, division, resume_url):
        self.firstname = firstname
        self.lastname = lastname
        self.phone = phone
        self.email = email
        self.division = division
        self.resume_url = resume_url


class Division:
    def __init__(self, id, name, email):
        self.id = id
        self.name = name
        self.email = email

    def serialize(self):
        return {
            "id": self.id,
            "name": self.name,
            "email": self.email
        }

# Create your views here.


divisions = []


@csrf_exempt
def index(request):
    divisions = get_divisions()
    if request.method == "POST":
        resume_url = request.POST["resume-url"]
        division_id = int(request.POST["division"])
        first_name = request.POST["firstname"]
        last_name = request.POST["lastname"]
        phone = request.POST["phone"]
        email = request.POST["email"]
        division = get_division(division_id)
        messages.info(request, '3')
        user = User(firstname=first_name, lastname=last_name,
                    phone=phone, email=email, division=division.name, resume_url=resume_url)
        messages.info(request, '4')
        if save_data(user):
            messages.info(request, '5')
            send_email(division.email, user)
            messages.info(request, '6')
            return render(request, "resumesubmission/index.html", {
                "message": "Successfully Applied!",
                "divisions": divisions
            })
        else:
            return render(request, "resumesubmission/index.html", {
                "divisions": divisions})
    else:
        return render(request, "resumesubmission/index.html", {
            "divisions": divisions})


def get_divisions():
    if not len(divisions):
        res = requests.get(
            'https://sqyh3ukdd6.execute-api.us-east-1.amazonaws.com/Dev/divisions').json()
        for div in res['body']:
            division = Division(
                id=div['ID'], name=div['Name'], email=div['Email'])
            divisions.append(division)
    return divisions


def get_division(div_id):
    division = next((div for div in divisions if div.id == div_id), None)
    return division


def send_email(email, user):
    ses_client = boto3.client("ses", region_name="us-east-1")

    CHARSET = "UTF-8"
    HTML_EMAIL_CONTENT = f'<html><head></head><ul><li>Name: {user.lastname},{user.firstname}</li><li>Number: {user.phone}</li><li>Email: {user.email}</li><li>Resume: <a href= {user.resume_url}>{user.resume_url}</a></li></ul></body></html>'

    response = ses_client.send_email(
        Destination={
            "ToAddresses": [
                email,
            ],
        },
        Message={
            "Body": {
                "Html": {
                    "Charset": CHARSET,
                    "Data": HTML_EMAIL_CONTENT,
                }
            },
            "Subject": {
                "Charset": CHARSET,
                "Data": "A new application has been submitted",
            },
        },
        Source="oms210@g.harvard.edu",
    )


@csrf_exempt
def upload(request):
    """Upload a file to an S3 bucket

    :param file_name: File to upload
    :param bucket: Bucket to upload to
    :param object_name: S3 object name. If not specified then file_name is used
    :return: True if file was uploaded, else False
    """
    if request.method == "POST" and request.FILES['myFile']:

        resume = request.FILES.get('myFile')
        fs = FileSystemStorage()
        filename = fs.save(resume.name, resume)
        uploaded_file_local_url = fs.path(filename)

        division_id = int(request.POST["division_id"])
        division = get_division(division_id)

        # Upload the file
        s3_client = boto3.client('s3', region_name="us-east-1")

        try:
            current_date = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            uploaded_file_name = f'{current_date}_{resume.name}'

            s3_bucket = f'resumes-oms210'

            response = s3_client.upload_file(
                uploaded_file_local_url, s3_bucket, '%s/%s' % (
                    division.name, uploaded_file_name))

            s3_file_url = f"https://s3.us-east-1.amazonaws.com/{s3_bucket}/{division.name}/{uploaded_file_name}"

        except ClientError as e:
            logging.error(e)

            return JsonResponse({"error: Error"}, status=201, safe=False)
        url_data = {
            "url": s3_file_url
        }
        return JsonResponse(json.dumps(url_data), status=201, safe=False)
    else:
        JsonResponse({"error: Error"}, status=201, safe=False)


def save_data(user):
    dynamodb = boto3.resource('dynamodb', region_name="us-east-1")
    try:
        table = dynamodb.Table('Users')
        if not table:
            table = dynamodb.create_table(
                TableName='Users',
                KeySchema=[
                    {
                        'AttributeName': 'id',
                        'KeyType': 'HASH'
                    },
                    {
                        'AttributeName': 'last_name',
                        'KeyType': 'RANGE'
                    }
                ],
                AttributeDefinitions=[
                    {
                        'AttributeName': 'id',
                        'AttributeType': 'S'
                    },
                    {
                        'AttributeName': 'first_name',
                        'AttributeType': 'S'
                    },
                    {
                        'AttributeName': 'last_name',
                        'AttributeType': 'S'
                    },
                    {
                        'AttributeName': 'phone',
                        'AttributeType': 'S'
                    },
                    {
                        'AttributeName': 'email',
                        'AttributeType': 'S'
                    },
                    {
                        'AttributeName': 'division',
                        'AttributeType': 'S'
                    },
                    {
                        'AttributeName': 'ResumeURL',
                        'AttributeType': 'S'
                    },
                ],
                ProvisionedThroughput={
                    'ReadCapacityUnits': 5,
                    'WriteCapacityUnits': 5
                }
            )
            table.wait_until_exists()

        table.put_item(
            Item={
                'ID': random.randint(0, 1000),
                'First_Name':  user.firstname,
                'Last_Name':  user.lastname,
                'Phone':  user.phone,
                'Email':  user.email,
                'Division': user.division,
                'ResumeURL': user.resume_url

            }
        )
    except ClientError as e:
        logging.error(e)
        return False
    return True
