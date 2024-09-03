using System.Text.Json;
using WebApplication1.Models;
using System.Drawing;
using System.Drawing.Imaging;

namespace WebApplication1.Services
{
    public class EmployeeService
    {
        private const string ApiUrl = "https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";

        public async Task CreateHtml()
        {
            var jsonData = await FetchDataFromApi(ApiUrl);

            var employees = ProcessData(jsonData);

            string htmlContent = GenerateHtml(employees);

            await File.WriteAllTextAsync("employees.html", htmlContent);
        }

        public async Task CreatePieChart()
        {
            var jsonData = await FetchDataFromApi(ApiUrl);

            var employees = ProcessData(jsonData);

            GeneratePieChart(employees, "employee_time_distribution.png");
        }

        private static async Task<string> FetchDataFromApi(string url)
        {
            using HttpClient client = new();

            HttpResponseMessage response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private static List<Employee> ProcessData(string jsonData)
        {
            var timeEntries = JsonSerializer.Deserialize<List<TimeEntry>>(jsonData);
            var employeeTotals = timeEntries
                .GroupBy(te => te.EmployeeName)
                .Select(g => new Employee
                {
                    Name = g.Key,
                    TotalTimeWorked = g.Sum(te => te.HoursWorked)
                })
                .Where(x => !string.IsNullOrEmpty(x.Name))
                .OrderByDescending(e => e.TotalTimeWorked)
                .ToList();

            return employeeTotals;
        }

        private static string GenerateHtml(List<Employee> employees)
        {
            var html = new System.Text.StringBuilder();
            html.Append("<html><head><style>");
            html.Append("table { width: 100%; border-collapse: collapse; }");
            html.Append("th, td { border: 1px solid black; padding: 8px; text-align: left; }");
            html.Append("tr.low-time { background-color: #f2dede; }");
            html.Append("</style></head><body>");
            html.Append("<table><thead><tr><th>Name</th><th>Total Time Worked</th></tr></thead><tbody>");

            foreach (var emp in employees)
            {
                var rowClass = emp.TotalTimeWorked < 100 ? "class='low-time'" : "";
                html.Append($"<tr {rowClass}><td>{emp.Name}</td><td>{emp.TotalTimeWorked}</td></tr>");
            }

            html.Append("</tbody></table></body></html>");
            return html.ToString();
        }

        private static void GeneratePieChart(List<Employee> employees, string filePath)
        {
            double totalHours = employees.Sum(e => e.TotalTimeWorked);

            int width = 1024;
            int height = 860;
            using (var bitmap = new Bitmap(width, height))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);

                // Draw the pie chart
                Rectangle rect = new Rectangle(100, 100, 600, 400);
                float startAngle = 0;
                foreach (var employee in employees)
                {
                    float sweepAngle = (float)(employee.TotalTimeWorked / totalHours * 360);
                    using (var brush = new SolidBrush(GetRandomColor()))
                    {
                        graphics.FillPie(brush, rect, startAngle, sweepAngle);
                    }
                    startAngle += sweepAngle;
                }

                int legendX = 720;
                int legendY = 100;
                int legendItemHeight = 30;
                foreach (var employee in employees)
                {
                    using (var brush = new SolidBrush(GetRandomColor()))
                    {
                        graphics.FillRectangle(brush, legendX, legendY, 20, 20);
                    }
                    graphics.DrawRectangle(Pens.Black, legendX, legendY, 20, 20);
                    graphics.DrawString(employee.Name, SystemFonts.DefaultFont, Brushes.Black, legendX + 30, legendY);
                    legendY += legendItemHeight;
                }

                bitmap.Save(filePath, ImageFormat.Png);
            }

            Console.WriteLine("Pie chart saved as 'piechart.png'.");
        }

        private static Color GetRandomColor()
        {
            Random random = new();
            return Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
        }
    }
}