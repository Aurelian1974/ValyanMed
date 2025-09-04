# Fi?iere de Test

Acest folder con?ine toate testele organizate pe tipuri.

## Structura

### UnitTests/
Teste unitare pentru:
- Services din Application layer
- Repositories din Infrastructure
- Domain models ?i valid?ri
- Utiliza?i conven?ia: `[ClassName]Tests.cs`

### BlazorComponents/
Teste pentru componente Blazor:
- Teste de render: `[ComponentName]Tests.razor`
- Code-behind tests: `[ComponentName]Tests.razor.cs`
- Utiliza?i bunit pentru testing

### IntegrationTests/
Teste de integrare pentru:
- API endpoints
- Database operations
- Authentication flows
- External services

## Conven?ii de Test

### Naming
- Test methods: `Should_ExpectedBehavior_When_Condition`
- Test classes: `[ClassName]Tests`

### Organizare
- Un test class per clas? testat?
- Grupa?i testele logice în regiuni
- Folosi?i `[Fact]` pentru teste simple, `[Theory]` pentru parametrizate

### Blazor Testing
```csharp
// Exemplu test component? Blazor
[Fact]
public void Should_RenderCorrectly_When_ValidDataProvided()
{
    // Arrange
    using var ctx = new TestContext();
    
    // Act
    var component = ctx.RenderComponent<MyComponent>();
    
    // Assert
    component.Find("h1").TextContent.Should().Be("Expected Title");
}
```