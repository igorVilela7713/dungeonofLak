# Prompts.md — Workflow de 3 Etapas (Research → Spec → Code)

> Workflow otimizado para desenvolvimento do roguelike 2D em Unity.
> Maximiza a janela de contexto limpando entre fases e usando arquivos como ponte
> (`PRD.md` → `SPEC.md` → implementação).
>
> O projeto é um jogo indie em Unity 2D, com foco em simplicidade, execução
> incremental e baixo acoplamento. Sempre preferir a menor solução que funcione.

---

## Visão Geral

```text
┌─────────────┐     /new       ┌─────────────┐     /new       ┌─────────────┐
│  1. Research │──────────────▶│  2. Spec     │──────────────▶│  3. Code     │
│              │  PRD.md       │              │  SPEC.md       │              │
│  Investigar  │  (ponte)      │  Planejar    │  (ponte)       │  Implementar │
└─────────────┘               └─────────────┘               └─────────────┘
```

**Regra de ouro:** nunca pule etapas. Nunca implemente direto sem `PRD.md` e `SPEC.md`.

---

## Etapa 1 — Research (Investigação)

### Quando usar

Nova feature, sistema novo, refatoração significativa, integração nova, bug complexo,
nova arma, novo inimigo, UI nova, save/load, câmera, dungeon, hub ou progressão.

### Prompt

```xml
<task>
Você precisa implementar [DESCRIÇÃO DA TAREFA] em um jogo roguelike 2D na Unity.
Faça uma investigação completa antes de propor qualquer código.
</task>

<context>
O projeto usa Unity 2D com C#.
É um projeto indie pequeno, com foco em simplicidade, modularidade e execução incremental.
Evite overengineering e abstrações prematuras.
Sempre considerar integração com Scene, GameObjects, Components, Prefabs, Animator,
Inspector e fluxo real de uso dentro da Unity.
</context>

<instructions>
1. Leia profundamente todos os arquivos relevantes do projeto antes de fazer afirmações.
2. Identifique scripts, prefabs, cenas e assets que serão afetados ou que servem de referência.
3. Encontre padrões de implementação similares já existentes no projeto para manter consistência.
4. Pesquise na internet a documentação oficial e exemplos recentes sobre a funcionalidade envolvida.
5. Considere o estágio atual do projeto: priorizar a solução mais simples possível.
6. Identifique dependências com outros sistemas futuros ou já existentes
   (ex: player, combate, inimigos, animação, room flow, UI, save).
7. Quando relevante, inclua detalhes de integração com Rigidbody2D, Collider2D,
   Animator, ScriptableObjects, Tilemap, Camera, Input e Prefabs.
8. Faça chamadas paralelas de ferramentas quando as leituras forem independentes.
</instructions>

<output_format>
Escreva o resultado em `PRD.md` seguindo EXATAMENTE esta estrutura:

## Objetivo
[O que queremos implementar e por quê — 1 parágrafo conciso]

## Arquivos Relevantes
| Arquivo | Relevância | Motivo |
|---------|------------|--------|
| Assets/Scripts/... | alta | [por quê] |

## Assets / Prefabs / Scenes Relevantes
| Caminho | Tipo | Motivo |
|--------|------|--------|
| Assets/Prefabs/... | Prefab | [por quê] |
| Assets/Scenes/... | Scene | [por quê] |

## Padrões Encontrados no Projeto
[Referências a scripts, prefabs ou fluxos já existentes]
[Inclua path + trecho relevante quando possível]

## Documentação Externa
[Resumo das referências técnicas encontradas e boas práticas]
[Incluir URLs quando disponíveis]

## Componentes Necessários
- [Lista de scripts, prefabs, componentes Unity, animações, colliders, etc.]

## Fluxo Esperado
[Descrever o comportamento esperado da funcionalidade dentro do jogo]

## Constraints
[Limitações e convenções do projeto: simplicidade, Unity 2D, pixel art, sem sistemas genéricos desnecessários, etc.]

## Riscos / Pontos de Atenção
[O que pode dar errado, edge cases, problemas comuns de integração, setup na Unity, etc.]

## Decisões a Tomar
[Perguntas em aberto que precisam de resposta antes de planejar]
</output_format>

<constraints>
- Não escreva NENHUM código nesta etapa — apenas análise e documentação.
- Não especule sobre arquivos que não leu.
- Seja exaustivo na leitura: profundamente, não superficialmente.
- Priorize soluções simples e adequadas para projeto indie pequeno.
- Considere sempre a integração real com a Unity Editor.
</constraints>
```

### Output

`PRD.md` preenchido com a investigação completa.

### Após esta etapa

→ Limpe o contexto (`/new`) e inicie a Etapa 2.

---

## Etapa 2 — Spec (Planejamento)

### Quando usar

Sempre que houver um `PRD.md` pronto da Etapa 1.

### Prompt

```xml
<task>
Leia `PRD.md` e gere um plano de implementação em `SPEC.md`.
Seja tático, preciso e orientado a arquivos e setup da Unity.
</task>

<context>
O projeto é um roguelike 2D em Unity/C#.
A solução deve ser pequena, clara, incremental e sem overengineering.
Os scripts devem ser fáceis de conectar no Inspector e de testar em Play Mode.
Também existe um arquivo `SETUP.md`, que deve ser atualizado explicando como configurar
a funcionalidade dentro da Unity passo a passo.
</context>

<instructions>
1. Liste TODOS os arquivos que precisam ser CRIADOS, com caminho completo.
2. Liste TODOS os arquivos que precisam ser MODIFICADOS.
3. Para CADA arquivo, descreva exatamente o que deve ser feito.
4. Inclua pseudocódigo ou snippets quando a implementação exigir padrão específico.
5. Explique como a funcionalidade será conectada na Unity:
   - em qual GameObject colocar cada script
   - quais campos configurar no Inspector
   - quais objetos/prefabs/colliders criar ou ajustar
6. Inclua uma seção específica de atualização do `SETUP.md`.
7. Crie um checklist de implementação com checkboxes (`- [ ]`) para cada passo.
8. Ao final, adicione uma seção `## Perguntas` com decisões que precisam de validação humana.
</instructions>

<constraints>
- Não implemente NADA — apenas planeje.
- Evite arquitetura exagerada.
- Não crie abstrações genéricas sem necessidade real.
- Pensar como projeto pequeno/indie.
- Especificar integração real com a Unity Editor.
- Sempre que possível, separar:
  - sistema
  - conteúdo
  - polish
</constraints>

<output_format>
## Arquivos a Criar
| Path | Tipo | Descrição |
|------|------|-----------|

## Arquivos a Modificar
| Path | Mudanças |
|------|----------|

## Setup na Unity
- [Passo a passo do que fazer no Editor]
- [GameObjects envolvidos]
- [Campos do Inspector]
- [Prefabs / Scenes / Animator / Colliders / Layers, se necessário]

## Atualização do SETUP.md
- [O que documentar no SETUP.md]
- [Passo a passo que deve ser adicionado]

## Checklist de Implementação
- [ ] Passo 1: [descrição]
  - Arquivo: `Assets/...`
  - O que fazer: [detalhe]
- [ ] Passo 2: [descrição]

## Perguntas
- [Pergunta 1 que precisa de decisão humana antes de implementar]

## Validação
- [ ] Scripts compilam sem erro no Unity
- [ ] Setup no Inspector está documentado
- [ ] Fluxo pode ser testado em Play Mode
- [ ] `SETUP.md` será atualizado com o passo a passo da feature
</output_format>
```

### Output

`SPEC.md` com plano completo, checklist de tarefas e instruções de integração.

### Annotation Cycle (Ciclo de Anotação)

Após gerar o `SPEC.md`, **não pule direto para a Etapa 3**.

1. Abra o `SPEC.md`
2. Adicione notas inline diretamente no arquivo
3. Envie de volta para a IA:

```xml
<task>
Adicionei notas inline no SPEC.md. Leia o documento e enderece TODAS as notas.
Atualize o documento de acordo.
</task>

<constraints>
Não implemente nada ainda — apenas atualize o spec.
</constraints>
```

1. Repita até o plano estar correto
2. Só então avance para a Etapa 3

### Após esta etapa

→ Limpe o contexto (`/new`) e inicie a Etapa 3.

---

## Etapa 3 — Code (Implementação)

### Quando usar

Sempre que houver um `SPEC.md` pronto e aprovado.

### Prompt

```xml
<task>
Implemente a `SPEC.md`.
Leia o arquivo e execute cada item do checklist na ordem definida.
</task>

<context>
O projeto é um roguelike 2D na Unity.
O código deve ser simples, legível, modular e fácil de configurar no Inspector.
O arquivo `SETUP.md` deve ser atualizado para explicar, fase por fase, como aplicar
a funcionalidade dentro da Unity Editor.
</context>

<instructions>
1. Siga o `SPEC.md` exatamente.
2. Marque cada item concluído no checklist com `- [x]`.
3. Crie ou atualize os arquivos exatamente nos caminhos descritos.
4. Explique onde colocar cada script na Unity.
5. Explique como configurar os campos no Inspector.
6. Atualize o `SETUP.md` com um passo a passo claro da implementação no Editor.
7. Ao final, explique como testar a funcionalidade em Play Mode.
8. Se houver qualquer ambiguidade que o spec não cobre, PARE e pergunte.
9. Não declare completo até todos os checkboxes estarem marcados.
</instructions>

<constraints>
- Siga o spec à risca.
- Não invente features extras.
- Não faça overengineering.
- Não crie sistemas genéricos sem necessidade.
- Não adicionar comentários desnecessários.
- Não misturar lógica de gameplay com UI sem necessidade.
- Sempre preferir a solução mínima que resolve o problema.
- Se algo no spec parecer errado, avise — não “corrija” sozinho.
</constraints>

<verification>
Antes de declarar pronto, verifique:
- [ ] Todos os checkboxes do SPEC.md estão marcados
- [ ] Scripts compilam sem erro no Unity
- [ ] Setup no Inspector foi explicado
- [ ] `SETUP.md` foi atualizado
- [ ] A funcionalidade pode ser testada em Play Mode
- [ ] Nenhum sistema desnecessariamente genérico foi criado
</verification>
```

### Output

Código implementado, `SETUP.md` atualizado, pronto para teste no Unity.

### Após esta etapa

- Se o spec foi alterado durante implementação → atualizar `SPEC.md`
- Se houver nova convenção ou decisão importante → registrar no documento apropriado do projeto
- Commitar apenas após validação humana

---

## Variações por Tipo de Tarefa

### Novo Sistema de Player / Combate

Etapa 1 (Research) — adicionar ao prompt:

```xml
<context>
Além do padrão, leia os scripts de player, combate, vida, animação e input já existentes.
Considere integração com Rigidbody2D, Collider2D, Animator e hitboxes.
Evite criar um framework de combate — implementar apenas o necessário.
</context>
```

### Nova Arma

Etapa 1 (Research) — adicionar ao prompt:

```xml
<context>
Além do padrão, leia os scripts de weapon/controller/damage e os prefabs relacionados.
Verifique se a arma deve reutilizar padrão existente ou exigir comportamento próprio.
Evite abstrações genéricas demais antes da segunda ou terceira arma real.
</context>
```

### Novo Inimigo

Etapa 1 (Research) — adicionar ao prompt:

```xml
<context>
Além do padrão, leia os scripts de inimigo, vida, movimento, detecção e dano.
Considere se o inimigo é apenas uma variante de stats/comportamento ou exige um fluxo novo.
</context>
```

### Animação / Animator

Etapa 1 (Research) — adicionar ao prompt:

```xml
<context>
Além do padrão, leia os scripts e assets ligados ao Animator, SpriteRenderer e estados do player/inimigo.
Considere também transições, parâmetros, condições e integração com movimento/combate.
</context>
```

### Câmera

Etapa 1 (Research) — adicionar ao prompt:

```xml
<context>
Além do padrão, leia os scripts de câmera e o setup atual da Main Camera.
Evite Cinemachine se o projeto ainda estiver em fase inicial.
Priorizar solução simples e estável com follow e smoothing mínimo.
</context>
```

### Save / Progressão / Hub

Etapa 1 (Research) — adicionar ao prompt:

```xml
<context>
Além do padrão, leia os scripts e assets ligados a save, hub, NPCs, armas desbloqueadas e progressão.
Separar claramente dados persistentes da run atual.
</context>
```

### Bug Fix

Para bugs simples, as 3 etapas podem ser condensadas:

```xml
<task>
Bug: [DESCRIÇÃO DO BUG]
</task>

<instructions>
1. Reproduza o problema lendo profundamente os arquivos relevantes.
2. Identifique a causa raiz — não trate apenas o sintoma.
3. Corrija com a menor mudança possível.
4. Explique quais scripts/objetos foram afetados.
5. Explique como validar no Unity Play Mode.
6. Se necessário, atualize o `SETUP.md` com observações relevantes.
</instructions>

<constraints>
- Não delete código sem entender a causa raiz.
- Não reestruture sistemas inteiros para corrigir bug pequeno.
- Não remova validações ou fluxos só para “fazer funcionar”.
- Se após algumas tentativas o problema continuar ambíguo, pare e documente as dúvidas.
</constraints>
```

### Referência Externa

Quando houver uma implementação de referência:

```xml
<task>
Quero implementar [FEATURE] seguindo o padrão abaixo.
</task>

<reference_implementation>
[COLE O CÓDIGO DE REFERÊNCIA AQUI]
</reference_implementation>

<instructions>
Leia o `PRD.md` e a implementação de referência acima.
Gere o `SPEC.md` adotando abordagem similar, adaptada ao nosso projeto Unity.
</instructions>
```

---

## ✅ Prompt melhorado (PT-BR)

Agora que a nova fase foi finalizada, siga rigorosamente os passos abaixo:

---

### 1. Criar uma nova branch

- O nome da branch deve refletir claramente a fase (usar nome descritivo)

---

### 2. Analisar as alterações

- Identificar todos os arquivos modificados
- Identificar todos os arquivos novos (criados)
- Identificar alterações em documentação, specs ou PRD (se houver)

---

### 3. Criar commits seguindo estas regras

#### a) Arquivos modificados

- Agrupar arquivos modificados em um ou mais commits lógicos  
- Utilizar mensagens de commit claras explicando o que foi alterado e o motivo  

#### b) Arquivos novos

- Devem ser commitados separadamente dos arquivos modificados  
- Agrupar de forma lógica (ex: arquivos da mesma feature juntos)  

#### c) Documentação / Specs / PRD

- **DEVEM ser commitados separadamente**  
- Inclui: arquivos de documentação, markdown, specs, PRD e qualquer artefato não relacionado diretamente ao código  
- A mensagem do commit deve indicar claramente que são mudanças de documentação  

#### d) Regras gerais

- **NÃO misturar** arquivos novos, modificados e documentação no mesmo commit  
- Manter commits pequenos, organizados e atômicos  
- Seguir boas práticas de mensagens de commit (claras e objetivas)  

---

### 4. Consciência de código

- Revisar a lógica existente antes de commitar  
- Adicionar comentários no código quando necessário para explicar decisões  
- Em caso de dúvida, **PERGUNTAR antes de executar qualquer ação**  

---

### 5. Pull Request

- Após finalizar os commits, criar um PR  

O PR deve conter:

- Resumo da nova fase  
- Principais mudanças realizadas  
- Decisões técnicas importantes  
- Pontos de atenção ou riscos  

---

### 6. Restrições importantes

- **NÃO fazer push direto na main**  
- **NÃO pular etapas**  
- **NÃO criar um único commit com tudo**  
- **NÃO commitar sem entender as mudanças**  

---

## Referências

- `PRD.md` — Output da Etapa 1 (Research)
- `SPEC.md` — Output da Etapa 2 (Spec)
- `SETUP.md` — Passo a passo de configuração no Unity Editor
- Documentação oficial Unity
- Documentação oficial dos packages usados no projeto
