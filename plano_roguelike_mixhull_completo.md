# 🧠 Plano de Execução (IA-Ready) — Roguelike: Aeson em Mixhull

## 📌 Stack Técnica (FIXA)

- Engine: Unity 2D
- Linguagem: C#
- Arte: LibreSprite (pixel art)
- Persistência: JSON local
- Dados: ScriptableObjects
- Tilemap: Unity Tilemap
- Input: Unity Input System (ou legacy)

---

## 📏 Regras de Implementação (CRÍTICAS)

- NÃO criar sistemas genéricos cedo demais
- Sempre usar placeholder antes de arte final
- Só abstrair quando houver 2+ usos reais
- Priorizar código simples e legível
- Separar sistema / conteúdo / polish
- Não misturar lógica com UI

---

# 🚀 Fase 1 — Setup (P0)

## Tarefa 1: Estrutura do projeto

- Criar pastas: Scenes, Scripts, Prefabs, Art, Animations, UI, Audio, Data

## Tarefa 2: Configurar pixel art

- Filter Mode: Point
- Compression: None
- Camera: Orthographic
- Pixel Perfect Camera

## Tarefa 3: Cena inicial

- Cena "Main"
- GameManager básico

---

# 🚀 FASE 2 — Player Core

## Prompt 2.1 — Player Movement

Você é um desenvolvedor Unity especialista em jogos 2D.

Implemente um PlayerController.

Requisitos:

- Movimento em 2D (horizontal e vertical)
- Usar Rigidbody2D
- Velocidade configurável
- Colisão funcional

Critérios:

- Player se move suavemente
- Não atravessa paredes

Código simples, sem overengineering.

---

## Prompt 2.2 — Player Health

Crie um sistema de vida para o player.

Requisitos:

- Classe PlayerHealth
- HP inicial configurável
- Método TakeDamage(int damage)
- Detectar morte

Critérios:

- Player perde vida corretamente
- Player morre ao chegar em 0

Sem UI.

---

## Prompt 2.3 — Direção do Player

Implemente sistema de direção do player.

Requisitos:

- Detectar direção de movimento
- Armazenar última direção válida

Critérios:

- Direção disponível para uso no ataque

---

# ⚔️ FASE 3 — Combate Base

## Prompt 3.1 — Sistema de Ataque

Implemente sistema de ataque.

Requisitos:

- Input de ataque
- Cooldown
- Trigger de ataque

Critérios:

- Player ataca ao pressionar input
- Não permite spam

---

## Prompt 3.2 — Hitbox

Implemente hitbox de ataque.

Requisitos:

- Hitbox temporária
- Detectar colisão com inimigos

Critérios:

- Hitbox ativa apenas durante ataque

---

## Prompt 3.3 — Aplicar Dano

Integre dano com inimigos.

Requisitos:

- Aplicar dano ao detectar hit
- Integrar com EnemyHealth

Critérios:

- Inimigo perde vida corretamente

---

# 👾 FASE 4 — Enemy Base

## Prompt 4.1 — Enemy Movement

Crie um inimigo que segue o player.

Requisitos:

- Detectar player
- Movimento em direção ao player

Critérios:

- Enemy segue o player corretamente

---

## Prompt 4.2 — Enemy Health

Crie sistema de vida do inimigo.

Requisitos:

- Classe EnemyHealth
- Receber dano
- Morrer ao chegar em 0

Critérios:

- Enemy morre corretamente

---

## Prompt 4.3 — Enemy Attack

Implemente ataque do inimigo.

Requisitos:

- Dano ao encostar no player

Critérios:

- Player perde vida ao contato

---

# 🧱 FASE 5 — Loop de Sala

## Prompt 5.1 — RoomController

Implemente RoomController.

Requisitos:

- Detectar inimigos na sala
- Controlar estado (em combate / limpo)

Critérios:

- Sala reconhece quando inimigos acabam

---

## Prompt 5.2 — Enemy Spawner

Crie sistema de spawn.

Requisitos:

- Spawnar inimigos ao entrar na sala

Critérios:

- Inimigos aparecem corretamente

---

## Prompt 5.3 — Porta

Implemente sistema de porta.

Requisitos:

- Porta fecha durante combate
- Porta abre após limpar sala

Critérios:

- Player só sai após vencer

---

# 🏛️ FASE 6 — Hub

## Prompt 6.1 — Scene Transition

Implemente transição de cena.

Requisitos:

- Ir do hub para dungeon

Critérios:

- Transição funciona sem erro

---

## Prompt 6.2 — NPC Base

Crie NPC interativo.

Requisitos:

- Detectar interação
- Mostrar texto simples

Critérios:

- Player consegue interagir

---

## Prompt 6.3 — Save System

Crie sistema de save.

Requisitos:

- Salvar dados básicos em JSON

Critérios:

- Dados persistem entre execuções

## Prompt 6.4 — Camera System

Implemente um sistema de câmera 2D que siga o player em Unity.

Contexto:

- Jogo roguelike 2D top-down
- Câmera deve ser simples (sem Cinemachine)
- Projeto ainda está em fase inicial

Requisitos:

- A câmera deve seguir o player automaticamente
- Manter o player centralizado na tela
- Movimento suave (interpolação / smoothing)
- Não alterar eixo Z da câmera
- Código desacoplado (não embutir lógica no Player)

Criar:

- CameraFollow.cs

Detalhes técnicos:

- Usar Transform do player como target
- Usar Lerp ou SmoothDamp para suavizar movimento
- Executar movimento em LateUpdate

Critérios de aceite:

- Câmera segue o player continuamente
- Movimento não é travado (não “teleporta”)
- Player permanece visível e centralizado
- Não há jitter ou tremedeira

Entrega esperada:

- Código completo
- Onde anexar o script (Main Camera)
- Como configurar o target no Inspector
- Como testar no Play Mode

Evitar:

- Cinemachine
- Sistemas complexos
- Overengineering

---

# 🗺️ FASE 7 — Dungeon Procedural e Progressão de Andares

## 🎯 Objetivo
Implementar a estrutura principal da dungeon com:
- geração procedural de andares
- progressão entre floors
- salas especiais (boss e reward)
- base para scaling de dificuldade

---

## Prompt 7.1 — FloorManager

Crie um sistema de andares para a dungeon.

### Contexto
- Jogo roguelike 2D
- Dungeon com 15 andares
- Andares 5, 10 e 15 são boss floors

### Requisitos
- Criar `FloorManager`
- Controlar o andar atual
- Método para avançar andar
- Identificar tipo do andar:
  - normal
  - boss
  - reward area (após boss)
- Após boss, ir para reward area antes do próximo andar

### Critérios
- Andar incrementa corretamente
- Boss floors detectados corretamente (5, 10, 15)
- Fluxo: boss → reward → próximo andar

### Evitar
- Lógica espalhada em múltiplos scripts
- Misturar com combate ou UI

---

## Prompt 7.2 — Procedural Room System

Crie um sistema procedural de salas para andares normais.

### Contexto
- Cada andar normal deve ser gerado proceduralmente
- Prioridade: simplicidade e estabilidade
- Pode usar salas pré-fabricadas (prefabs)

### Requisitos
- Criar `DungeonGenerator` ou `RoomSystem`
- Gerar múltiplas salas conectadas
- Tipos mínimos de sala:
  - start
  - combat
  - exit
- Garantir caminho válido do início ao fim
- Permitir navegação entre salas

### Critérios
- Cada andar gera layout diferente
- Sempre existe caminho jogável
- Player consegue navegar entre salas

### Escopo inicial
- Grid simples
- Conexão lógica entre salas
- Sem algoritmo complexo

### Evitar
- Procedural complexo demais
- Mapas impossíveis
- Misturar com spawn ou boss logic

---

## Prompt 7.3 — Boss Floor System

Crie sistema de andares de boss.

### Contexto
- Andares 5, 10 e 15 são boss floors
- Não usam geração procedural normal
- Devem carregar arena específica

### Requisitos
- Detectar boss floors
- Carregar ou gerar arena de boss
- Criar fluxo pós-boss
- Encaminhar para reward area
- Preparar suporte para múltiplos bosses

### Critérios
- Boss floors funcionam corretamente
- Não usam sistema de salas normal
- Após vitória → reward area

### Evitar
- Misturar com dungeon generator
- Hardcode espalhado

---

## Prompt 7.4 — Reward / Upgrade Area

Crie área de recompensa pós-boss.

### Contexto
- Área segura após boss
- Sem combate
- Upgrade ainda não definido

### Requisitos
- Criar `RewardArea`
- Player entra após boss
- Estrutura para upgrade futuro
- Pode usar placeholder

### Critérios
- Área sem combate
- Fluxo correto após boss
- Pronto para expansão futura

### Evitar
- Sistema completo de upgrade agora
- Misturar com hub

---

## Prompt 7.5 — Difficulty Scaling

Implemente escala de dificuldade por andar.

### Contexto
- Dificuldade aumenta conforme o player avança
- Afeta inimigos normais e bosses

### Requisitos
- Aumentar HP e dano por andar
- Sistema centralizado
- Permitir ajuste fácil (multiplicadores)

### Critérios
- Dificuldade progressiva
- Valores fáceis de balancear
- Funciona para normal + boss

### Evitar
- Fórmulas complexas
- Lógica espalhada em vários scripts

---

# 🪓 FASE 8 — Armas

## Prompt 8.1 — Weapon System

Crie sistema de armas modular.

Requisitos:

- WeaponData
- WeaponController

Critérios:

- Troca de arma funciona

---

## Prompt 8.2 — Spear

Implemente lança.

Requisitos:

- Alcance maior
- Ataque linear

---

## Prompt 8.3 — Axe

Implemente machado.

Requisitos:

- Ataque lento
- Alto dano

---

# 👾 FASE 9 — Inimigos Avançados

## Prompt 9.1 — Enemy Types

Crie variações de inimigos.

Requisitos:

- Rápido
- Pesado
- Ranged

Critérios:

- Gameplay diferente

---

## Prompt 9.2 — Enemy Behavior

Melhore comportamento.

Requisitos:

- Padrões diferentes

---

# 🎁 FASE 10 — Recompensas

## Prompt 10.1 — Reward System

Crie sistema de recompensa.

Requisitos:

- Escolha após combate

---

## Prompt 10.2 — Upgrades

Crie upgrades temporários.

Requisitos:

- Buffs de run

Critérios:

- Não persistem após morte

---

# 🧙 FASE 11 — NPCs

## Prompt 11.1 — Shop / Blacksmith

Crie NPC funcional.

Requisitos:

- Compra ou upgrade

---

## Prompt 11.2 — NPC Progression

Implemente progressão.

Requisitos:

- Desbloqueio de NPCs

---

# 🔊 FASE 13 — UI e Áudio

## Prompt 13.1 — UI

Crie UI básica.

Requisitos:

- Barra de vida
- Andar atual

---

## Prompt 13.2 — Audio

Adicione áudio.

Requisitos:

- Som de ataque
- Som de dano

---

# 🛠️ FASE 14 — Polimento

## Prompt 14.1 — Game Feel

Implemente efeitos.

Requisitos:

- Knockback
- Particles
- Camera shake

---

## Prompt 14.2 — Balance

Ajuste balanceamento.

Requisitos:

- Dano
- Vida
- Ritmo

---

# 📦 FASE 15 — Demo

## Prompt 15.1 — Vertical Slice

Finalize demo jogável.

Requisitos:

- Hub funcional
- 3 andares
- 2 armas
- Loop completo
