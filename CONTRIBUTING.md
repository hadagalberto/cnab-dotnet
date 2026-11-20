# Contribuindo

Obrigado pelo interesse no Cnab.NET! Este documento resume o que se espera de
uma contribuição.

## Regras de ouro

1. **Nada é inventado.** Cada posição de layout e cada algoritmo de dígito
   verificador precisa citar o manual oficial (nome, versão e URL) em
   comentário XML e em `docs/layouts.md`. Não copie regra "de cabeça" ou de
   bibliotecas de terceiros sem confirmar na fonte oficial.
2. **Sem stubs.** Sem `NotImplementedException`, sem TODO pendente em código
   entregue, sem testes desativados ou simulados. Se algo não puder ser
   implementado ainda, ele simplesmente não entra.
3. **Os testes não enfraquecem.** Se um teste falha, corrige-se a causa
   raiz. Nunca se ajusta o teste para ele passar, exceto quando o próprio
   teste está matematicamente errado (nesse caso, explique no commit).

## Fluxo

1. Abra uma issue descrevendo a mudança (ou comente numa existente).
2. Fork/branch a partir de `main`.
3. Implemente com os padrões do repositório:
   - C# `latest`, nullable habilitado, warnings como erros;
   - documentação XML em todo membro público da biblioteca, citando a fonte
     oficial da regra;
   - namespaces/termos em português quando espelham o vocabulário bancário
     (remessa, retorno, nosso número, cedente, fator de vencimento).
4. Rode o mínimo barulhento:

   ```bash
   dotnet build Cnab.slnx -c Release
   dotnet test tests/Cnab.Tests
   dotnet format Cnab.slnx --verify-no-changes
   ```

5. Novos layouts de banco exigem vetores de teste do manual oficial (golden
   files) e atualização de `docs/layouts.md`.
6. Mudanças na CLI devem vir acompanhadas de testes em `CliTests` cobrindo
   tanto o caminho válido quanto o inválido (códigos de saída 0, 1 e 2). A
   CLI é uma ferramenta local: não publicar pacotes em NuGet.
7. Novos campos/registros devem entrar também nos geradores FsCheck de
   `ArbitrariosLayouts`, para que a propriedade de comprimento exato, o
   round-trip e os totais de trailer continuem cobrindo **todos** os layouts.
8. O teste de 1 milhão de linhas (`DesempenhoParseUmMilhaoTests`) tem limite
   generoso de 60 s. Se ele falhar, investigue a regressão de desempenho no
   motor; nunca aumente o limite para esconder o problema.
9. PRs são revisados com foco em: fonte citada, qualidade dos testes e
   aderência às convenções.

## Commit

Use [Conventional Commits](https://www.conventionalcommits.org/pt-br/v1.0.0/)
(`feat:`, `fix:`, `docs:`, `test:`, `refactor:` ...). Commits pequenos e
atômicos são preferidos.

## Licença

Ao contribuir, você concorda que sua contribuição será licenciada sob a MIT
(o projeto não publica pacotes em NuGet por decisão do SPEC).
