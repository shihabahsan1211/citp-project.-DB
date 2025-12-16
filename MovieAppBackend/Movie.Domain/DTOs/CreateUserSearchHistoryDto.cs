public class CreateUserSearchHistoryDto
{
    public string QueryText { get; set; } = null!;
    public DateTime? SearchedOn { get; set; }
}
