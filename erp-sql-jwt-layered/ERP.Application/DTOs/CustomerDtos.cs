namespace ERP.Application.DTOs
{
    public record CustomerReadDto
    {
        public int Id { get; init; }
        public string Name { get; init; }
        public string? Email { get; init; }
        public string? Phone { get; init; }

        public CustomerReadDto(int id, string name, string? email,string? phone)
        {
            Id = id;
            Name = name;
            Email = email;
            Phone = phone;
        }
    }

    public record CustomerCreateDto(string Name, string? Email, string? Phone);
    public record CustomerUpdateDto(string Name, string? Email, string? Phone);
}
