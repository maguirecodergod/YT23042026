using System.Net.Http.Json;

namespace HTT.BlazorWasm.App.Pages
{
    public class DemoEmployee
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public bool IsActive { get; set; }
        public DateTime JoinDate { get; set; }
        public double PerformanceRating { get; set; }
    }

    public partial class DemoTable : HTTComponentBase
    {
        [Inject] private HttpClient HttpClient { get; set; } = default!;

        private HTTTable<DemoEmployee>? _table;
        private HashSet<DemoEmployee> _selectedItems = new();

        // Filter states
        private string _searchKeyword = string.Empty;
        private string _filterDepartment = string.Empty;
        private string _filterStatus = string.Empty;

        // In-memory mock database
        private List<DemoEmployee> _mockDatabase = new();

        protected override async Task OnInitializedAsync()
        {
            try 
            {
                // Try to load initial data from "API" (static JSON file)
                var apiData = await HttpClient.GetFromJsonAsync<List<DemoEmployee>>("sample-data/employees.json");
                if (apiData != null)
                {
                    _mockDatabase.AddRange(apiData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading sample data: {ex.Message}");
            }

            // Still generate more data to test performance/pagination with 1000 records
            GenerateMockData(1000 - _mockDatabase.Count); 
            
            await RefreshTable();
        }



        private void GenerateMockData(int count)
        {
            var random = new Random(42); // fixed seed for consistency
            var firstNames = new[] { "James", "Mary", "Robert", "Patricia", "John", "Jennifer", "Michael", "Linda", "David", "Elizabeth", "William", "Barbara", "Richard", "Susan", "Joseph", "Jessica", "Thomas", "Sarah", "Charles", "Karen" };
            var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin" };
            var departments = new[] { "Engineering", "Sales", "Marketing", "HR", "Finance" };

            for (int i = 1; i <= count; i++)
            {
                var firstName = firstNames[random.Next(firstNames.Length)];
                var lastName = lastNames[random.Next(lastNames.Length)];

                _mockDatabase.Add(new DemoEmployee
                {
                    Id = 10000 + i,
                    Name = $"{firstName} {lastName}",
                    Email = $"{firstName.ToLower()}.{lastName.ToLower()}@htt.example.com",
                    Department = departments[random.Next(departments.Length)],
                    Salary = random.Next(45000, 150000),
                    IsActive = random.NextDouble() > 0.15, // 85% active
                    JoinDate = DateTime.Today.AddDays(-random.Next(0, 3650)), // up to 10 years ago
                    PerformanceRating = Math.Round((random.NextDouble() * 2) + 3, 1) // 3.0 to 5.0
                });
            }
        }

        // Simulates an API call with pagination, filtering, and sorting
        private async Task<(IEnumerable<DemoEmployee> Items, int TotalCount)> LoadServerData(TableState<DemoEmployee> state)
        {
            // Simulate network latency (200ms)
            await Task.Delay(200);

            var query = _mockDatabase.AsQueryable();

            // 1. Apply Filters
            if (!string.IsNullOrWhiteSpace(_searchKeyword))
            {
                var keyword = _searchKeyword.ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(keyword) || x.Email.ToLower().Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(_filterDepartment))
            {
                query = query.Where(x => x.Department == _filterDepartment);
            }

            if (!string.IsNullOrWhiteSpace(_filterStatus))
            {
                bool isActive = _filterStatus == "true";
                query = query.Where(x => x.IsActive == isActive);
            }

            // Update filters in state (optional, just to keep sync if we want)
            state.Filters["Search"] = _searchKeyword;
            state.Filters["Department"] = _filterDepartment;
            state.Filters["Status"] = _filterStatus;

            // 2. Sorting
            if (state.Sorts.Any())
            {
                var sort = state.Sorts.First();

                if (sort.FieldName == nameof(DemoEmployee.Id))
                    query = sort.Direction == SortDirection.Ascending ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id);
                else if (sort.FieldName == nameof(DemoEmployee.Name))
                    query = sort.Direction == SortDirection.Ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name);
                else if (sort.FieldName == nameof(DemoEmployee.Department))
                    query = sort.Direction == SortDirection.Ascending ? query.OrderBy(x => x.Department) : query.OrderByDescending(x => x.Department);
                else if (sort.FieldName == nameof(DemoEmployee.Salary))
                    query = sort.Direction == SortDirection.Ascending ? query.OrderBy(x => x.Salary) : query.OrderByDescending(x => x.Salary);
                else if (sort.FieldName == nameof(DemoEmployee.JoinDate))
                    query = sort.Direction == SortDirection.Ascending ? query.OrderBy(x => x.JoinDate) : query.OrderByDescending(x => x.JoinDate);
            }
            else
            {
                // Default sort
                query = query.OrderBy(x => x.Id);
            }

            // 3. Count Total (before pagination)
            int totalCount = query.Count();

            // 4. Pagination
            var pagedData = query
                .Skip((state.PageIndex - 1) * state.PageSize)
                .Take(state.PageSize)
                .ToList();

            return (pagedData, totalCount);
        }

        private void HandleSelectionChanged(HashSet<DemoEmployee> selection)
        {
            _selectedItems = selection;
            StateHasChanged();
        }

        private void HandleRowClick(DemoEmployee item)
        {
            Console.WriteLine($"Row clicked: {item.Name}");
        }

        private async Task RefreshTable()
        {
            if (_table != null)
            {
                await _table.RefreshAsync();
            }
        }

        private async Task ClearFilters()
        {
            _searchKeyword = string.Empty;
            _filterDepartment = string.Empty;
            _filterStatus = string.Empty;
            await RefreshTable();
        }
    }
}
