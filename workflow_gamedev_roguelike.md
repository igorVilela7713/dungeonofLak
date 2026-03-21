# 🎮 Workflow: Game Dev Spec-Driven (Unity Roguelike)

## 💡 Princípios Fundamentais

### 1. Gestão de Contexto
- Use `/new` entre etapas
- Evite contexto acumulado

### 2. Execução Incremental
- Sempre implementar UMA tarefa por vez
- Validar antes de avançar

### 3. Código Simples > Código Inteligente
- Evitar overengineering
- Não criar abstrações prematuras

### 4. Separação Clara
- Sistema ≠ Conteúdo ≠ Polish

---

# 🔄 Fluxo de Trabalho (3 Etapas)

---

## 1. Pesquisa e PRD

### Prompt

"""
Estamos desenvolvendo um jogo roguelike 2D na Unity (C#).

Precisamos implementar a seguinte funcionalidade:

[TAREFA]

Antes de codar, faça uma análise completa:

1. Analise como essa funcionalidade deve ser implementada em Unity 2D
2. Liste os componentes necessários:
   - scripts
   - prefabs
   - componentes Unity (Rigidbody2D, Collider, etc)
3. Sugira a abordagem mais simples possível (evitando overengineering)
4. Considere que o projeto ainda está em fase inicial (não criar sistemas complexos)
5. Se aplicável, identifique dependências com sistemas futuros (ex: combate, inimigos, etc)
6. Busque padrões de implementação similares que já usamos para mantermos a consistência.
7. Pesquise na internet a documentação oficial de [BIBLIOTECA/TECNOLOGIA] e busque exemplos de melhores práticas no Stack Overflow ou GitHub.

Ao final gere um arquivo `PRD.md` com:

- Objetivo da funcionalidade
- Componentes necessários
- Estrutura sugerida
- Fluxo básico de funcionamento
- Riscos ou pontos de atenção

Seja direto e prático.
"""

---

## 2. Especificação (SPEC)

### Prompt

"""
Leia o arquivo `PRD.md`.

Agora, aja como um Engenheiro de Software Sênior focado em Unity 2D e crie um `SPEC.md`.

A especificação deve ser EXECUTÁVEL e detalhada.

Inclua:

### Arquivos
- Liste cada arquivo a ser criado com caminho completo (ex: Assets/Scripts/Player/PlayerController.cs)

### Estrutura
- Defina classes e responsabilidades
- Não criar abstrações desnecessárias

### Lógica
- Descreva o fluxo do sistema passo a passo

### Pseudocódigo
- Inclua lógica detalhada suficiente para implementação direta

### Integração
- Explique como conectar os scripts na Unity (GameObject, Inspector, etc)

Regras:
- Código simples
- Nada genérico demais
- Nada de arquitetura exagerada
- Pensar em um projeto pequeno/indie

Seja específico e direto.
"""

---

## 3. Implementação

### Prompt

"""
Leia o arquivo `SPEC.md` e implemente a funcionalidade.

Regras obrigatórias:

- Seguir exatamente o SPEC
- Código simples e legível
- Evitar overengineering
- Não criar sistemas genéricos
- Não adicionar features extras

Entrega esperada:

1. Código completo dos arquivos
2. Nome correto dos arquivos
3. Explicação de onde colocar cada script na Unity
4. Como configurar no Inspector
5. Como testar a funcionalidade

Se houver qualquer ambiguidade, pergunte antes de implementar.
"""