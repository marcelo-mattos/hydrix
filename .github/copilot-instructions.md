# Copilot Instructions

## Project Guidelines
- Nos testes e mocks deste projeto, reutilizar classes de apoio em `Materializer.Mocks.cs` (como `TestDbCommand`, `TestDbParameter`, `TestDbParameterCollection`) e manter summaries XML em tudo, seguindo o padrão do projeto. Inclua XML summaries completos (com `<param>` e `<returns>` quando aplicável) ao documentar métodos, aplicando também às subclasses internas e a todos os seus métodos e propriedades, incluindo métodos privados. Isso inclui a documentação de propriedades e campos privados, inclusive nas subclasses internas.