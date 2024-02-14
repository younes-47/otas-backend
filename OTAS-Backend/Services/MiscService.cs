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
                        break;
                    case "Pending HR's Approval":
                        explicitStatusHistory.Status = "Approved";
                        explicitStatusHistory.CreateDate = statusHistory.CreateDate;
                        explicitStatusHistory.DeciderFirstName = statusHistory.DeciderFirstName;
                        explicitStatusHistory.DeciderLastName = statusHistory.DeciderLastName;
                        break;
                    case "Pending Finance Department's Approval":
                        explicitStatusHistory.Status = "Approved";
                        explicitStatusHistory.CreateDate = statusHistory.CreateDate;
                        explicitStatusHistory.DeciderFirstName = statusHistory.DeciderFirstName;
                        explicitStatusHistory.DeciderLastName = statusHistory.DeciderLastName;
                        break;
                    case "Pending General Director's Approval":
                        explicitStatusHistory.Status = "Approved";
                        explicitStatusHistory.CreateDate = statusHistory.CreateDate;
                        explicitStatusHistory.DeciderFirstName = statusHistory.DeciderFirstName;
                        explicitStatusHistory.DeciderLastName = statusHistory.DeciderLastName;
                        break;
                    case "Pending Vice President's Approval":
                        explicitStatusHistory.Status = "Approved";
                        explicitStatusHistory.CreateDate = statusHistory.CreateDate;
                        explicitStatusHistory.DeciderFirstName = statusHistory.DeciderFirstName;
                        explicitStatusHistory.DeciderLastName = statusHistory.DeciderLastName;
                        break;
                    case "Pending Treasury's Validation":
                        if (explicitStatusHistory.DeciderFirstName != null)
                        {
                            explicitStatusHistory.Status = "Approved";
                            explicitStatusHistory.CreateDate = statusHistory.CreateDate;
                            explicitStatusHistory.DeciderFirstName = statusHistory.DeciderFirstName;
                            explicitStatusHistory.DeciderLastName = statusHistory.DeciderLastName;
                        }
                        else
                        {
                            explicitStatusHistory.Status = "Resubmitted";
                            explicitStatusHistory.CreateDate = statusHistory.CreateDate;
                            explicitStatusHistory.DeciderFirstName = "";
                            explicitStatusHistory.DeciderLastName = "";
                        }
                        break;
                    case "Preparing Funds":
                        explicitStatusHistory.Status = "Approved";
                        explicitStatusHistory.CreateDate = statusHistory.CreateDate;
                        explicitStatusHistory.DeciderFirstName = statusHistory.DeciderFirstName;
                        explicitStatusHistory.DeciderLastName = statusHistory.DeciderLastName;
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

        public Aspose.Pdf.Table GenerateTripsTableForDocuments(List<TripDTO> trips)
        {
            Aspose.Pdf.Table table = new Aspose.Pdf.Table
            {
                Border = new Aspose.Pdf.BorderInfo(Aspose.Pdf.BorderSide.All, .5f, Color.Black),
                DefaultCellBorder = new Aspose.Pdf.BorderInfo(Aspose.Pdf.BorderSide.All, .5f, Color.Black),
                ColumnWidths = "15% 15% 15% 15% 15% 15% 15%",
                Alignment = HorizontalAlignment.FullJustify

            };
            

            Aspose.Pdf.Row headersRows = table.Rows.Add();
            headersRows.Cells.Add("Depart");
            headersRows.Cells.Add("Arrive");
            headersRows.Cells.Add("Methode de transport");
            headersRows.Cells.Add("Unite");
            headersRows.Cells.Add("Valeur");
            headersRows.Cells.Add("Autoroute");
            headersRows.Cells.Add("Frais Estime");

            foreach (TripDTO trip in trips)
            {
                Aspose.Pdf.Row valuesRow = table.Rows.Add();
                valuesRow.Cells.Add(trip.DepartureDate.ToString("dd/MM/yyyy"));
                valuesRow.Cells.Add(trip.ArrivalDate.ToString("dd/MM/yyyy"));
                valuesRow.Cells.Add(trip.TransportationMethod.ToString());
                valuesRow.Cells.Add(trip.Unit);
                valuesRow.Cells.Add(trip.Value.ToString().FormatWith(new CultureInfo("fr-FR")));
                valuesRow.Cells.Add(trip.HighwayFee.ToString().FormatWith(new CultureInfo("fr-FR")));
                valuesRow.Cells.Add(trip.EstimatedFee.ToString().FormatWith(new CultureInfo("fr-FR")));

            }
            
            return table;
        }


        public Aspose.Pdf.Table GenerateSignatoriesTableForDocuments(List<Signatory> signers)
        {
            Aspose.Pdf.Table table = new Aspose.Pdf.Table
            {
                Border = new Aspose.Pdf.BorderInfo(Aspose.Pdf.BorderSide.All, .5f, Color.Black),
                DefaultCellBorder = new Aspose.Pdf.BorderInfo(Aspose.Pdf.BorderSide.All, .5f, Color.Black),
                ColumnWidths = "20% 20% 20% 20% 20%",
                Alignment = HorizontalAlignment.FullJustify

            };


            Aspose.Pdf.Row headersRows = table.Rows.Add();
            headersRows.Cells.Add("BENEFICIAIRE");
            headersRows.Cells.Add("MANAGER DEPARTEMENT");
            headersRows.Cells.Add("TRESORERIE");
            headersRows.Cells.Add("DIRECTEUR FINANCIER");
            headersRows.Cells.Add("DIRECTEUR GENERAL");

            Aspose.Pdf.Row valuesRow = table.Rows.Add();

            // Requester
            valuesRow.Cells.Add("");

            if (signers.Any(s => s.Level == "MG") == true)
            {
                valuesRow.Cells.Add(signers.Where(s => s.Level == "MG").Select(s => $"{s.FirstName} {s.LastName}").First());
            }
            else
            {
                valuesRow.Cells.Add("");
            }

            // TR
            valuesRow.Cells.Add("");

            if (signers.Any(s => s.Level == "FM") == true)
            {
                valuesRow.Cells.Add(signers.Where(s => s.Level == "FM").Select(s => $"{s.FirstName} {s.LastName}").First());
            }
            else
            {
                valuesRow.Cells.Add("");
            }

            if (signers.Any(s => s.Level == "GD") == true)
            {
                valuesRow.Cells.Add(signers.Where(s => s.Level == "GD").Select(s => $"{s.FirstName} {s.LastName}").First());
            }
            else
            {
                valuesRow.Cells.Add("");
            }


            return table;
        }
    }
}
