using AutoMapper.Execution;
using OTAS.DTO.Get;
using OTAS.Interfaces.IService;
using OTAS.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Humanizer;
using Aspose.Cells;
using System.Text.RegularExpressions;
using Aspose.Pdf;
using Aspose.Pdf.Text;

namespace OTAS.Services
{
    /*
     
        THIS SERVICE IS INTENDED FOR CALLING HELPER FUNCTIONS THAT CAN BE USED THROUGHOUT THE APPLICATION
        IT IS NOT RELATED TO ANY SPECIFIC SERVICE

    */
    public class MiscService : IMiscService
    {
        public decimal CalculateExpensesEstimatedTotal(List<Expense> expenses)
        {
            decimal estimatedTotal = 0;
            foreach (Expense expense in expenses)
            {
                estimatedTotal += expense.EstimatedFee;
            }

            return estimatedTotal;
        }

        public decimal CalculateTripEstimatedFee(Trip trip)
        {
            decimal estimatedFee = 0;
            const decimal MILEAGE_ALLOWANCE = 2.5m; // 2.5DH PER KM
            if (trip.Unit == "KM")
            {
                estimatedFee += trip.HighwayFee + (trip.Value * MILEAGE_ALLOWANCE);
            }
            else
            {
                estimatedFee += trip.Value;
            }


            return estimatedFee;
        }

        public decimal CalculateTripsEstimatedTotal(List<Trip> trips)
        {
            decimal estimatedTotal = 0;
            const decimal MILEAGE_ALLOWANCE = 2.5m; // 2.5DH PER KM
            foreach (Trip trip in trips)
            {
                if (trip.Unit == "KM")
                {
                    estimatedTotal += trip.HighwayFee + (trip.Value * MILEAGE_ALLOWANCE);
                }
                else
                {
                    estimatedTotal += trip.Value;
                }
            }

            return estimatedTotal;
        }


        public int GenerateRandomNumber(int length)
        {
            int[] _decimalNumbers = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Random _random = new();

            var confirmationString = new StringBuilder(length);

            confirmationString.Append(_decimalNumbers[_random.Next(1,10)]); // Avoid generating 0 at the beginning

            for (int i = 1; i < length; i++)
            {
                confirmationString.Append(_decimalNumbers[_random.Next(10)]);
            }

            return int.Parse(confirmationString.ToString());
        }

        public string GenerateRandomString(int length)
        {
            char[] _base62chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();

            Random _random = new();

            var sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
                sb.Append(_base62chars[_random.Next(36)]); //Base36 will only include numbers and capital letters, if you want to include small letters change it to 62

            return sb.ToString();
        }

        public List<StatusHistoryDTO> IllustrateStatusHistory(List<StatusHistoryDTO> statusHistories)
        {
            List<StatusHistoryDTO> illustratedStatusHistory = statusHistories;

            for (int i = illustratedStatusHistory.Count - 1; i >= 0; i--)
            {
                StatusHistoryDTO statusHistory = illustratedStatusHistory[i];
                StatusHistoryDTO explicitStatusHistory = new();
                switch (statusHistory.Status)
                {
                    case "Pending Manager's Approval":
                        explicitStatusHistory.Status = "Submitted";
                        explicitStatusHistory.CreateDate = statusHistory.CreateDate;
                        explicitStatusHistory.Total = statusHistory.Total;
                        break;
                    case "Pending HR's Approval":
                        explicitStatusHistory.Status = "Approved";
                        explicitStatusHistory.CreateDate = statusHistory.CreateDate;
                        explicitStatusHistory.DeciderFirstName = statusHistory.DeciderFirstName;
                        explicitStatusHistory.DeciderLastName = statusHistory.DeciderLastName;
                        explicitStatusHistory.Total = statusHistory.Total;
                        break;
                    case "Pending Finance Department's Approval":
                        explicitStatusHistory.Status = "Approved";
                        explicitStatusHistory.CreateDate = statusHistory.CreateDate;
                        explicitStatusHistory.DeciderFirstName = statusHistory.DeciderFirstName;
                        explicitStatusHistory.DeciderLastName = statusHistory.DeciderLastName;
                        explicitStatusHistory.Total = statusHistory.Total;
                        break;
                    case "Pending General Director's Approval":
                        explicitStatusHistory.Status = "Approved";
                        explicitStatusHistory.CreateDate = statusHistory.CreateDate;
                        explicitStatusHistory.DeciderFirstName = statusHistory.DeciderFirstName;
                        explicitStatusHistory.DeciderLastName = statusHistory.DeciderLastName;
                        explicitStatusHistory.Total = statusHistory.Total;
                        break;
                    case "Pending Vice President's Approval":
                        explicitStatusHistory.Status = "Approved";
                        explicitStatusHistory.CreateDate = statusHistory.CreateDate;
                        explicitStatusHistory.DeciderFirstName = statusHistory.DeciderFirstName;
                        explicitStatusHistory.DeciderLastName = statusHistory.DeciderLastName;
                        explicitStatusHistory.Total = statusHistory.Total;
                        break;
                    case "Pending Treasury's Validation":
                        if (explicitStatusHistory.DeciderFirstName != null)
                        {
                            explicitStatusHistory.Status = "Approved";
                            explicitStatusHistory.CreateDate = statusHistory.CreateDate;
                            explicitStatusHistory.DeciderFirstName = statusHistory.DeciderFirstName;
                            explicitStatusHistory.DeciderLastName = statusHistory.DeciderLastName;
                            explicitStatusHistory.Total = statusHistory.Total;
                        }
                        else
                        {
                            explicitStatusHistory.Status = "Resubmitted";
                            explicitStatusHistory.CreateDate = statusHistory.CreateDate;
                            explicitStatusHistory.DeciderFirstName = "";
                            explicitStatusHistory.DeciderLastName = "";
                            explicitStatusHistory.Total = statusHistory.Total;
                        }
                        break;
                    case "Preparing Funds":
                        explicitStatusHistory.Status = "Approved";
                        explicitStatusHistory.CreateDate = statusHistory.CreateDate;
                        explicitStatusHistory.DeciderFirstName = statusHistory.DeciderFirstName;
                        explicitStatusHistory.DeciderLastName = statusHistory.DeciderLastName;
                        explicitStatusHistory.Total = statusHistory.Total;
                        break;
                    default:
                        continue;
                }
                illustratedStatusHistory.Insert(i, explicitStatusHistory);
            }

            return illustratedStatusHistory;
        }

        public string GetDeciderLevelByStatus(int status, string requestType)
        {
            string level = "";
            switch (status)
            {
                case 2:
                    level = "MG";
                    break;
                case 3:
                    if(requestType == "OM")
                    {
                        level = "HR";
                    }
                    else
                    {
                        level = "MG";
                    }
                    break;
                case 4:
                    level = "FM";
                    break;
                case 5:
                    level = "GD";
                    break;
                case 7:
                    if (requestType == "OM")
                    {
                        level = "GD";
                    }
                    else
                    {
                        level = "VP";
                    }
                    break;
                case 8:
                    level = "GD";
                    break;
                default:
                    break;
            }

            return level;
        }

        public bool IsRequestDecidable(string deciderUsername, string  nextDeciderUsername, string latestStatus)
        {
            if(deciderUsername == nextDeciderUsername && latestStatus != "Funds Collected" 
                && latestStatus != "Finalized" && latestStatus != "Approved")
                return true;

            return false;
        }


        public Xceed.Document.NET.Table GenerateExpesnesTableForDocuments(Xceed.Words.NET.DocX docx, List<ExpenseDTO> expenses)
        {
            Xceed.Document.NET.Table table = docx.AddTable(1, 2);
            table.SetColumnWidth(0, 400, false);
            table.SetColumnWidth(1, 150, false);
            Xceed.Document.NET.Row headersRows = table.Rows[0];
            headersRows.Cells[0].Paragraphs.First().Append("Description").Bold().Font("Arial").FontSize(11);
            headersRows.Cells[1].Paragraphs.First().Append("Frais Estimés").Bold().Font("Arial").FontSize(11);


            foreach (ExpenseDTO expense in expenses)
            {
                Xceed.Document.NET.Row valuesRow = table.InsertRow();
                valuesRow.Cells[0].Paragraphs.First().Append(expense.Description).Font("Arial");
                valuesRow.Cells[1].Paragraphs.First().Append(expense.Currency + " " + expense.EstimatedFee.ToString().FormatWith(new CultureInfo("fr-FR"))).Font("Arial").FontSize(11);
            }

            return table;
        }



    }
}
