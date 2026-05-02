namespace HTT.BlazorWasm.App.Contracts
{
    public class UserBaseModel
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Position { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public CGenderType? Gender { get; set; }
    }

    public class UserPickerLoadRequest
    {
        public string SearchText { get; set; } = string.Empty;
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class UserPickerLoadResponse
    {
        public List<UserBaseModel> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }
}