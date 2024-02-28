using OTAS.DTO.Get;
using OTAS.Interfaces.IService;
using OTAS.Models;
using System.Globalization;
using System.Text;
using Humanizer;
using System.Net.Mail;
using System.Net;
using OTAS.Interfaces.IRepository;

namespace OTAS.Services
{
    /*
     
        THIS SERVICE IS INTENDED FOR CALLING HELPER FUNCTIONS THAT CAN BE USED THROUGHOUT THE APPLICATION
        IT IS NOT RELATED TO ANY SPECIFIC SERVICE

    */
    public class MiscService : IMiscService
    {
        private readonly IDeciderRepository _deciderRepository;
        public MiscService(IDeciderRepository deciderRepository)
        {
            _deciderRepository = deciderRepository;
        }
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
                        if (statusHistory.DeciderFirstName != null)
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

        public string GetDeciderLevelByStatus(int status, bool? isRequestOM = false)
        {
            string level = "";
            switch (status)
            {
                case 2:
                    level = "MG";
                    break;
                case 3:
                    if(isRequestOM == true)
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
                    if (isRequestOM == true)
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
                case 13:
                    level = "GD";
                    break;
                default:
                    break;
            }

            return level;
        }

        public bool IsRequestDecidable(int deciderUserId, int?  nextDeciderUserId, string latestStatus)
        {
            if(deciderUserId == nextDeciderUserId && latestStatus != "Funds Collected" 
                && latestStatus != "Finalized" && latestStatus != "Approved" && 
                latestStatus != "Returned for missing evidences" && latestStatus != "Returned"
                && latestStatus  != "Rejected")
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

        public Xceed.Document.NET.Table GenerateExpesnesTableForLiquidationDocuments(Xceed.Words.NET.DocX docx, List<ExpenseDTO> expenses)
        {
            Xceed.Document.NET.Table table = docx.AddTable(1, 2);
            table.SetColumnWidth(0, 400, false);
            table.SetColumnWidth(1, 150, false);
            Xceed.Document.NET.Row headersRows = table.Rows[0];
            headersRows.Cells[0].Paragraphs.First().Append("Description").Bold().Font("Arial").FontSize(11);
            headersRows.Cells[1].Paragraphs.First().Append("Montant dépensé").Bold().Font("Arial").FontSize(11);


            foreach (ExpenseDTO expense in expenses)
            {
                Xceed.Document.NET.Row valuesRow = table.InsertRow();
                valuesRow.Cells[0].Paragraphs.First().Append(expense.Description).Font("Arial");
                valuesRow.Cells[1].Paragraphs.First().Append(expense.Currency + " " + expense.ActualFee.ToString().FormatWith(new CultureInfo("fr-FR"))).Font("Arial").FontSize(11);
            }

            return table;
        }

        public Xceed.Document.NET.Table GenerateTripsTableForDocuments(Xceed.Words.NET.DocX docx, List<TripDTO> trips)
        {
            Xceed.Document.NET.Table table = docx.AddTable(1, 9);
            Xceed.Document.NET.Row headersRows = table.Rows[0];

            headersRows.Cells[0].Paragraphs.First().Append("Départ").Bold().Font("Arial").FontSize(10);
            headersRows.Cells[1].Paragraphs.First().Append("Destination").Bold().Font("Arial").FontSize(10);
            headersRows.Cells[2].Paragraphs.First().Append("Date de départ").Bold().Font("Arial").FontSize(10);
            headersRows.Cells[3].Paragraphs.First().Append("Date d'arrivée").Bold().Font("Arial").FontSize(10);
            headersRows.Cells[4].Paragraphs.First().Append("Transport").Bold().Font("Arial").FontSize(10);
            headersRows.Cells[5].Paragraphs.First().Append("Unité").Bold().Font("Arial").FontSize(10);
            headersRows.Cells[6].Paragraphs.First().Append("Valeur").Bold().Font("Arial").FontSize(10);
            headersRows.Cells[7].Paragraphs.First().Append("Autoroute").Bold().Font("Arial").FontSize(10);
            headersRows.Cells[8].Paragraphs.First().Append("Frais Estimés").Bold().Font("Arial").FontSize(10);

            foreach (TripDTO trip in trips)
            {
                Xceed.Document.NET.Row valuesRow = table.InsertRow();
                valuesRow.Cells[0].Paragraphs.First().Append(trip.DeparturePlace).Font("Arial").FontSize(10);
                valuesRow.Cells[1].Paragraphs.First().Append(trip.Destination).Font("Arial").FontSize(10);
                valuesRow.Cells[2].Paragraphs.First().Append(trip.DepartureDate.ToString("dd/MM/yyyy")).Font("Arial").FontSize(10);
                valuesRow.Cells[3].Paragraphs.First().Append(trip.ArrivalDate.ToString("dd/MM/yyyy")).Font("Arial").FontSize(10);
                valuesRow.Cells[4].Paragraphs.First().Append(trip.TransportationMethod).Font("Arial").FontSize(10);
                valuesRow.Cells[5].Paragraphs.First().Append(trip.Unit).Font("Arial").FontSize(10);
                valuesRow.Cells[6].Paragraphs.First().Append(trip.Value.ToString().FormatWith(new CultureInfo("fr-FR"))).Font("Arial").FontSize(10);
                if(trip.HighwayFee > 0)
                {                  
                    valuesRow.Cells[7].Paragraphs.First().Append(trip.HighwayFee.ToString().FormatWith(new CultureInfo("fr-FR"))).Font("Arial").FontSize(10);
                }
                else
                {
                    valuesRow.Cells[7].Paragraphs.First().Append("N/A").Font("Arial").FontSize(10);
                }
                valuesRow.Cells[8].Paragraphs.First().Append(trip.EstimatedFee.ToString().FormatWith(new CultureInfo("fr-FR"))).Font("Arial").FontSize(10);

            }

            return table;
        }

        public Xceed.Document.NET.Table GenerateTripsTableForLiquidationDocuments(Xceed.Words.NET.DocX docx, List<TripDTO> trips)
        {
            Xceed.Document.NET.Table table = docx.AddTable(1, 9);
            Xceed.Document.NET.Row headersRows = table.Rows[0];

            headersRows.Cells[0].Paragraphs.First().Append("Départ").Bold().Font("Arial").FontSize(10);
            headersRows.Cells[1].Paragraphs.First().Append("Destination").Bold().Font("Arial").FontSize(10);
            headersRows.Cells[2].Paragraphs.First().Append("Date de départ").Bold().Font("Arial").FontSize(10);
            headersRows.Cells[3].Paragraphs.First().Append("Date d'arrivée").Bold().Font("Arial").FontSize(10);
            headersRows.Cells[4].Paragraphs.First().Append("Transport").Bold().Font("Arial").FontSize(10);
            headersRows.Cells[5].Paragraphs.First().Append("Unité").Bold().Font("Arial").FontSize(10);
            headersRows.Cells[6].Paragraphs.First().Append("Valeur").Bold().Font("Arial").FontSize(10);
            headersRows.Cells[7].Paragraphs.First().Append("Autoroute").Bold().Font("Arial").FontSize(10);
            headersRows.Cells[8].Paragraphs.First().Append("Montant dépensé").Bold().Font("Arial").FontSize(10);

            foreach (TripDTO trip in trips)
            {
                Xceed.Document.NET.Row valuesRow = table.InsertRow();
                valuesRow.Cells[0].Paragraphs.First().Append(trip.DeparturePlace).Font("Arial").FontSize(10);
                valuesRow.Cells[1].Paragraphs.First().Append(trip.Destination).Font("Arial").FontSize(10);
                valuesRow.Cells[2].Paragraphs.First().Append(trip.DepartureDate.ToString("dd/MM/yyyy")).Font("Arial").FontSize(10);
                valuesRow.Cells[3].Paragraphs.First().Append(trip.ArrivalDate.ToString("dd/MM/yyyy")).Font("Arial").FontSize(10);
                valuesRow.Cells[4].Paragraphs.First().Append(trip.TransportationMethod).Font("Arial").FontSize(10);
                valuesRow.Cells[5].Paragraphs.First().Append(trip.Unit).Font("Arial").FontSize(10);
                valuesRow.Cells[6].Paragraphs.First().Append(trip.Value.ToString().FormatWith(new CultureInfo("fr-FR"))).Font("Arial").FontSize(10);
                if (trip.HighwayFee > 0)
                {
                    valuesRow.Cells[7].Paragraphs.First().Append(trip.HighwayFee.ToString().FormatWith(new CultureInfo("fr-FR"))).Font("Arial").FontSize(10);
                }
                else
                {
                    valuesRow.Cells[7].Paragraphs.First().Append("N/A").Font("Arial").FontSize(10);
                }
                valuesRow.Cells[8].Paragraphs.First().Append(trip.ActualFee.ToString().FormatWith(new CultureInfo("fr-FR"))).Font("Arial").FontSize(10);

            }

            return table;
        }

        public string GenerateEmailBodyFrench(string requestType, int requestId, string deciderFullName)
        {
            string link = "localhost:3000/decide-on-requests";
            string requestHeader = "";
            switch (requestType)
            {
                case "OM":
                    link += "/decide-on-ordre-mission";
                    requestHeader = "Ordre de mission";
                    break;
                case "AV":
                    link += "/decide-on-avance-voyage";
                    requestHeader = "Avance de voyage";
                    break;
                case "AC":
                    link += "/decide-on-avance-caisse";
                    requestHeader = "Avance de caisse";
                    break;
                case "DC":
                    link += "/decide-on-depense-caisse";
                    requestHeader = "Dépense de caisse";
                    break;
                case "LQ":
                    link += "/decide-on-liquidation";
                    requestHeader = "Liquidation";
                    break;
                default:
                    break;
            }
            string emailMessage = $"<h4>Bonjour {deciderFullName},</h4>" + "<br/>" +
                $"<p>Vous avez une nouvelle demande en attente de votre validation!</p>" + "<br/>" +
                $"<p>{requestHeader} #{requestId}</p>" + "<br/>" +
                $"<p><a href='{link}'>Cliquez ici pour plus de details</a></p>" + "<br/>" +
                $"<p>Cordialement,</p>";

            return emailMessage;
        }

        public string GenerateEmailBodyEnglish(string requestType, int requestId, string deciderFullName)
        {
            string link = "localhost:3000/decide-on-requests";
            string requestHeader = "";
            switch (requestType)
            {
                case "OM":
                    link += "/decide-on-ordre-mission";
                    requestHeader = "Mission Order";
                    break;
                case "AV":
                    link += "/decide-on-avance-voyage";
                    requestHeader = "Travel Advance";
                    break;
                case "AC":
                    link += "/decide-on-avance-caisse";
                    requestHeader = "Cash Advance";
                    break;
                case "DC":
                    link += "/decide-on-depense-caisse";
                    requestHeader = "Cash Expense";
                    break;
                case "LQ":
                    link += "/decide-on-liquidation";
                    requestHeader = "Liquidation";
                    break;
                default:
                    break;
            }
            string emailMessage = $"<h4>Hello {deciderFullName},</h4>" + "<br/>" +
                $"<p>You have a new request pending your approval!</p>" + "<br/>" +
                $"<p>{requestHeader} #{requestId}</p>" + "<br/>" +
                $"<p><a href='{link}'>Click here for more details</a></p>" + "<br/>" +
                $"<p>Sincerely,</p>";

            return emailMessage;
        }

        public async Task<bool> SendMailToDecider(string requestType,int? nextDecider, int requestId)
        {
            string emailBody = "";

            if (nextDecider != null)
            {
                UserDTO deciderInfo = await _deciderRepository.GetDeciderInfoForEmailNotificationAsync((int)nextDecider);
                if (deciderInfo.PreferredLanguage == "fr")
                {
                    emailBody = GenerateEmailBodyFrench(requestType, requestId, $"{deciderInfo.FirstName} {deciderInfo.LastName}");
                }
                else
                {
                    emailBody = GenerateEmailBodyEnglish(requestType, requestId, $"{deciderInfo.FirstName} {deciderInfo.LastName}");
                }
            }
            else
            {
                return false;
            }
            try
            {
                string userName = "otas_alert@dicastalma.com";
                string password = "Dikamorocco@05";
                MailMessage msg = new();
                msg.To.Add(new MailAddress("anass.assila.7@gmail.com"));
                msg.From = new MailAddress(userName);
                msg.Subject = "New request pending approval";
                msg.Body = emailBody;
                msg.IsBodyHtml = true;
                using (SmtpClient client = new()
                {
                    Host = "smtp.office365.com",
                    Credentials = new NetworkCredential(userName, password),
                    Port = 587,
                    EnableSsl = true,
                })
                {
                    client.UseDefaultCredentials = false;
                    await client.SendMailAsync(msg);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }
    }
}
