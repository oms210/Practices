using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MerchantsGuideBook
{
    public enum RomanNumbers
    {
        I = 1,
        V = 5,
        X = 10,
        L = 50,
        C = 100,
        D = 500,
        M = 1000
    }
    public enum RomanNumberIndexes
    {
        I,
        V,
        X,
        L,
        C,
        D,
        M
    }
    public partial class Form3 : Form
    {
        List<Mapping> mappings;
        List<TradeQuery> trades;
        List<Item> items;
        string regularExpression = string.Empty;
        StringBuilder sbRegExTradeQuery;
        StringBuilder sbRegExQuery;
        /*
             * I=0
             * V=1
             * X=2
             * L=3
             * C=4
             * D=5
             * M=6
             */
        bool[,] validRomanOperations =  { { false, true, true, false, false, false, false}, //I
                            { false, false, false, false, false, false, false}, //V
                                { false, false, false, true, true, false, false}, //X
                                    { false, false, false, false, false, false, false}, //L
                                        { false, false, false, false, false, true, true}, //C
                                            { false, false, false, false, false, false, false}, //D
                                                { false, false, false, false, false, false, false} //M
                         };

        public Form3()
        {
            InitializeComponent();
        }

        private void lstBoxPrices_SelectedValueChanged(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            sbRegExTradeQuery = new StringBuilder();
            sbRegExTradeQuery.Append("^(?<galactic>(");
            if (mappings != null || mappings.Count > 0)
                mappings.Clear();
            GroupBox form = (GroupBox)this.Controls["grpRomans"];
            Mapping mapping;
            foreach (string roman in Enum.GetNames(typeof(RomanNumbers)))
            {
                mapping = new Mapping
                {
                    // String name = Enum.GetName(typeof(RomanNumbers), number);
                    RomanNumber = roman,
                    InterGalacticNumber = ((TextBox)form.Controls[("txt" + roman)]).Text.Trim()
                };
                mappings.Add(mapping);
                sbRegExTradeQuery.Append(@"(\b" + mapping.InterGalacticNumber + @"\b\s*){0,}");
            }
            sbRegExTradeQuery.Append(@")) units of (?<product>\b(Silver|Gold|Iron|Dirt)\b) are worth (?<credits>\d+) credits$");
            MessageBox.Show("saved");
        }
        private void btnAnswer_Click(object sender, EventArgs e)
        {
            lblAnswer.Text = string.Empty;
            string answer = "I have no idea what you are talking about";
            lblAnswer.ForeColor = Color.Red;
            sbRegExQuery = new StringBuilder();
            sbRegExQuery.Append(@"^how \b(much|many)\b? (?<credits>\bcredits\b)? is (?<galactic>(");


            foreach (Mapping mapping in mappings)
            {
                sbRegExQuery.Append(@"(\b" + mapping.InterGalacticNumber + @"\b\s*){0,}");
            }
            sbRegExQuery.Append(@")) (?<product>\b(Silver|Gold|Iron|Dirt)\b)*$");
            string galacticNumber = string.Empty;
            string product = string.Empty;
            int arabicNumber = 0;
            string credits = string.Empty;
            bool validQuery = VerifyQuery(txtQuery.Text.Trim(), sbRegExQuery.ToString(), ref product, ref arabicNumber, ref credits, ref galacticNumber);
            if (validQuery)
            {
                if (!string.IsNullOrWhiteSpace(product))
                {
                    Item item = items.Find(i => i.Name.Equals(product, StringComparison.OrdinalIgnoreCase));
                    if (item != null)
                    {
                        if (!string.IsNullOrWhiteSpace(credits))
                        {
                            answer = galacticNumber + " " + product + " is " + Convert.ToString(item.UnitPrice * arabicNumber) + " credits.";
                        }
                    }
                }
                else
                    answer = galacticNumber + " is " + arabicNumber;
                lblAnswer.ForeColor = Color.Green;

            }
            lblAnswer.Text = answer;
        }
        private bool VerifyNumerals(string galacticNumerals, ref List<ArabicNumber> arabicNumbers)
        {
            bool isValid = false;
            string[] galacticNumbers = galacticNumerals.Split(' ');


            ArabicNumber arabicNumber;
            StringBuilder translatedRomanNumbers = new StringBuilder();
            string romanNumber = string.Empty;

            foreach (string number in galacticNumbers)
            {

                string romanSymbol = mappings.Find(m => m.InterGalacticNumber.Equals(number, StringComparison.OrdinalIgnoreCase)).RomanNumber;
                translatedRomanNumbers.Append(romanSymbol);
                arabicNumber = new ArabicNumber
                {
                    Index = (int)Enum.Parse(typeof(RomanNumberIndexes), romanSymbol),
                    RomanNumber = romanSymbol,
                    Value = (int)Enum.Parse(typeof(RomanNumbers), romanSymbol)
                };
                arabicNumbers.Add(arabicNumber);
            }
            romanNumber = translatedRomanNumbers.ToString();
            Regex romanValidationExpression = new Regex(@"^M{0,4}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})$", RegexOptions.IgnoreCase);
            if (romanValidationExpression.IsMatch(romanNumber))
                isValid = true;

            return isValid;
        }
        private int GetArabicNumber(ref List<ArabicNumber> arabicNumbers)
        {
            int totalRomanNumberValue = 0;
            int lastRomanIndex = -1;
            int lastRomanNumberValue = 0;

            if (arabicNumbers.Count > 0)
            {
                foreach (ArabicNumber number in arabicNumbers)
                {
                    if (lastRomanIndex == -1)//first iteration
                    {
                        totalRomanNumberValue += number.Value;
                    }
                    else if (number.Index > lastRomanIndex)
                    {

                        totalRomanNumberValue = (totalRomanNumberValue - lastRomanNumberValue) + (number.Value - lastRomanNumberValue);
                    }
                    else if (number.Index <= lastRomanIndex)
                        totalRomanNumberValue += number.Value;

                    lastRomanIndex = number.Index;
                    lastRomanNumberValue = number.Value;
                }
            }
            return totalRomanNumberValue;
        }
        private bool VerifyQuery(string source, string regularExpression, ref string product, ref int arabicNumber, ref string credits, ref string galacticNumber)
        {
            bool isValid = false;
            Regex pattern = new Regex(regularExpression, RegexOptions.IgnoreCase);
            Match match = pattern.Match(source); 
            if (match.Success)
            {
                List<ArabicNumber> arabicNumbers = new List<ArabicNumber>();
                galacticNumber = match.Groups["galactic"].Value;
                if (VerifyNumerals(galacticNumber, ref arabicNumbers))
                {
                    if (match.Groups["product"]!=null)
                        product = match.Groups["product"].Value.Trim();
                    if (match.Groups["credits"] != null)
                        credits = match.Groups["credits"].Value.Trim();
                    arabicNumber = GetArabicNumber(ref arabicNumbers);
                    isValid = true;
                }
            }
            return isValid;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string product = string.Empty;
            int arabicNumber = 0;
            string credits = string.Empty;
            string galacticNumber = string.Empty;
            bool validQuery = VerifyQuery(txtPrice.Text.Trim(), sbRegExTradeQuery.ToString(), ref product, ref arabicNumber, ref credits, ref galacticNumber);
            if (validQuery)
            {
                int itemCredits = Convert.ToInt32(credits);
                TradeQuery query = new TradeQuery
                {
                    TradeID = trades.Count + 1,
                    Query = txtPrice.Text.Trim(),
                    Product = product,
                    ArabicNumber = arabicNumber,
                    Credits = itemCredits
                };
                trades.Add(query);
                Item item = new Item
                {
                    Name = product,
                    UnitPrice = Convert.ToDouble(itemCredits) / Convert.ToDouble(arabicNumber)
                };
                items.Add(item);

                MessageBox.Show("saved");
            }
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            mappings = new List<Mapping>();
            trades = new List<TradeQuery>();
            items = new List<Item>();
         
            
            
        }

      
    }
}
