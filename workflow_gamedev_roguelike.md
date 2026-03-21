# 🎮 Workflow: Game Dev Spec-Driven (Unity Roguelike)

## 💡 Princípios Fundamentais

1. **Gestão de Contexto**
- Use `/clear` entre etapas
- Evite acúmulo de contexto desnecessário

2. **Execução Incremental**
- Sempre implementar UMA tarefa por vez
- Validar antes de avançar

3. **Código Simples > Código Inteligente**
- Evitar overengineering
- Não criar abstrações prematuras

4. **Separação Clara**
- Sistema ≠ Conteúdo ≠ Polish

---

# 🔄 Fluxo de Trabalho (3 Etapas)

## 1. Pesquisa e PRD

### Prompt
"""
Precisamos implementar a seguinte funcionalidade em um jogo roguelike 2D na Unity:

[TAREFA]

Antes de codar:

1. Analise como essa funcionalidade normalmente é implementada em Unity 2D
2. Identifique os principais componentes necessários (scripts, prefabs, etc)
3. Busque boas práticas para evitar overengineering
4. Gere um PRD.md com:
   - objetivo
   - componentes necessários
   - estrutura sugerida
   - riscos

Código deve ser simples e direto.
"""

---

## 2. Especificação (SPEC)

### Prompt
"""
Leia o PRD.md e gere um SPEC.md.

Inclua:

- Arquivos a serem criados (com caminho)
- Estrutura de classes
- Responsabilidades de cada script
- Fluxo lógico do sistema
- Pseudocódigo

Evite complexidade desnecessária.
"""

---

## 3. Implementação

### Prompt
"""
Leia o SPEC.md e implemente.

Regras:
- Código simples e modular
- Sem overengineering
- Seguir exatamente o SPEC
- Se houver dúvida, perguntar antes

Explique rapidamente as decisões.
"""

---

# 🚀 Prompts Iniciais (Execução Direta)

## 🎯 Prompt 1 — Estrutura do Projeto

"""
Crie a estrutura inicial de um projeto Unity 2D roguelike.

Pastas:
- Scenes
- Scripts
- Prefabs
- Art
- Animations
- UI
- Audio
- Data

Explique rapidamente o propósito de cada uma.
"""

---

## 🎯 Prompt 2 — Player Movement

"""
Implemente um PlayerController 2D.

Requisitos:
- Movimento horizontal e vertical
- Rigidbody2D
- Velocidade configurável

Critérios:
- Movimento fluido
- Sem atravessar colisores

Código simples.
"""

---

## 🎯 Prompt 3 — Player Health

"""
Crie um sistema de vida para o player.

Requisitos:
- Classe PlayerHealth
- HP inicial configurável
- Método TakeDamage
- Detectar morte

Critérios:
- HP reduz corretamente
- Morte detectada

Sem UI ainda.
"""

---

## 🎯 Prompt 4 — Combate (Espada)

"""
Implemente combate básico com espada.

Requisitos:
- Input de ataque
- Hitbox temporária
- Cooldown
- Dano aplicado

Critérios:
- Player ataca
- Enemy recebe dano
- Sem spam infinito

Código simples.
"""

---

## 🎯 Prompt 5 — Enemy Básico

"""
Crie um inimigo básico.

Requisitos:
- Seguir player
- Causar dano ao encostar
- Receber dano
- Morrer

Critérios:
- Enemy funcional
"""

---

## 🎯 Prompt 6 — Sala de Combate

"""
Implemente RoomController.

Requisitos:
- Spawn de inimigos
- Porta trava durante combate

Critérios:
- Porta abre ao limpar sala
- Combate inicia corretamente
"""
