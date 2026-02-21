namespace RabbitFlow.Domain;

public record Order(int Id);

public record User(Guid Id, string Name, string Email);