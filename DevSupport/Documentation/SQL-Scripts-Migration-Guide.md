# Ghid de Migrare - Scripturi SQL

## ?? Rezumat Migrare

**Data**: 4 septembrie 2025  
**Ac?iune**: Mutarea scripturilor SQL din `Shared\scripts` în `DevSupport\SqlScripts`  
**Status**: ? Completat?

## ?? Ce s-a întâmplat

### Înainte
```
Shared\
  ??? scripts\
      ??? *.sql (45 scripturi)
      ??? *.txt (12 fi?iere documenta?ie)
```

### Dup?
```
DevSupport\
  ??? SqlScripts\
  ?   ??? Functions\ (3 fi?iere)
  ?   ??? Tables\ (5 fi?iere)  
  ?   ??? StoredProcedures\ (25 fi?iere)
  ?   ??? Setup\ (4 fi?iere)
  ?   ??? SeedData\ (2 fi?iere)
  ?   ??? Migrations\ (6 fi?iere)
  ?   ??? Maintenance\ (4 fi?iere)
  ?   ??? Triggers\ (1 fi?ier)
  ?   ??? Tests\ (4 fi?iere)
  ?   ??? Legacy\ (2 fi?iere mari)
  ?   ??? Views\ (3 fi?iere)
  ?   ??? README.md
  ??? Documentation\
      ??? *.txt (12 fi?iere documenta?ie)
```

## ?? Beneficii

1. **Organizare Logic?**: Scripturile sunt organizate pe categorii func?ionale
2. **Embedded Resources**: Scripturile sunt incluse ca resurse în aplica?ie
3. **Mentenan?? U?oar?**: Structura faciliteaz? g?sirea ?i modificarea scripturilor
4. **Separarea Responsabilit??ilor**: DevSupport con?ine toate asset-urile de dezvoltare
5. **Documenta?ie Complet?**: README.md ?i ghiduri pentru dezvoltatori

## ?? Configura?ie Tehnic?

### DevSupport.csproj
```xml
<ItemGroup>
  <EmbeddedResource Include="SqlScripts\**\*.sql" />
  <Content Include="Documentation\**\*.md" />
  <Content Include="Documentation\**\*.txt" />
</ItemGroup>
```

### Accesarea Scripturilor din Cod
```csharp
public class SqlScriptLoader
{
    private readonly Assembly _assembly = typeof(SqlScriptLoader).Assembly;
    
    public string LoadScript(string category, string scriptName)
    {
        var resourceName = $"DevSupport.SqlScripts.{category}.{scriptName}";
        using var stream = _assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
```

## ?? Instruc?iuni pentru Echipa de Dezvoltare

### Ad?ugarea de Scripturi Noi
1. Plasa?i scriptul în directorul potrivit din `DevSupport\SqlScripts\`
2. Urma?i conven?ia de denumire existent?
3. Actualiza?i README.md dac? este necesar

### Modificarea Scripturilor Existente
1. G?si?i scriptul în categoria corespunz?toare
2. Efectua?i modific?rile
3. Testa?i folosind scripturile din `Tests\`

### Deployment
Scripturile sunt automat incluse în build ca embedded resources ?i pot fi accesate programatic pentru deployment automatizat.

## ?? Note Importante

- **Nu mai folosi?i** `Shared\scripts` - acest director a fost ?ters
- **Toate referin?ele** c?tre scripturile SQL trebuie actualizate s? pointeze c?tre DevSupport
- **Scripturile Legacy** (`script.sql`, `SP.sql`) sunt p?strate pentru referin?? dar nu se recomand? folosirea lor

## ?? Verificare Post-Migrare

- [x] Scripturile SQL sunt organizate în DevSupport\SqlScripts
- [x] Fi?ierele .txt sunt mutate în DevSupport\Documentation  
- [x] Directorul Shared\scripts a fost ?ters
- [x] DevSupport.csproj include scripturile ca EmbeddedResource
- [x] README.md este actualizat cu noua structur?
- [x] Documenta?ia pentru dezvoltatori este disponibil?

---
*Migrare efectuat? de: DevSupport Team*  
*Pentru întreb?ri sau probleme, consulta?i documenta?ia sau contacta?i echipa de dezvoltare.*