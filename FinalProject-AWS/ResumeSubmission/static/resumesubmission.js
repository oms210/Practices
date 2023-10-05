document.addEventListener('DOMContentLoaded', function () {
    let division = document.querySelector('#division');
    let divisions = document.querySelector('#sel-divisions');
    division.value = divisions.value;
    divisions.onchange = function () {
        division.value = divisions.value;
    }
    let fileupload = document.querySelector('#file-upload');
    if (fileupload != undefined)
        fileupload.addEventListener('change', event => {
            handleUpload(event)
        })
    
});
const handleUpload = event => {
    const files = event.target.files;
    let selected_file = document.querySelector('#selected-file-name');
    selected_file.innerHTML = '';
    if (files.length > 0) {

        selected_file.innerHTML = files[0].name;
        let division = document.querySelector('#division');
        const formData = new FormData();
        formData.append('myFile', files[0]);
        formData.append('division_id', division.value);
        fetch('upload', {
                method: 'POST',
                body: formData
            })
            .then(response => response.json())
            .then(resume => {
                if (!resume.hasOwnProperty('error')) {

                    let resume_url = document.querySelector('#resume-url');
                    resume_url.value = JSON.parse(resume).url;
                    show_msg('Successfully uploaded', false);

                } else {
                    show_msg('Upload Error', true);

                }
            })
            .catch(error => {
                console.error(error)
                show_msg('Upload Error', true);
            });
    }
}

function show_msg(msg, isError) {

    let msgDiv = document.querySelector('#msg');
    if (isError)
        msgDiv.classList.add('alert-danger');
    else
        msgDiv.classList.add('alert-success');
    msgDiv.classList.remove('fade-out');
    setTimeout(() => {
        msgDiv.classList.remove('alert-danger');
        msgDiv.classList.remove('alert-success');
        document.querySelector('#msg').innerHTML = '';
        msgDiv.classList.add('fade-out');
        msgDiv.classList.remove('fade-in');
    }, 3000);
    document.querySelector('#msg').innerHTML = msg;
    msgDiv.classList.toggle('fade-in');

}