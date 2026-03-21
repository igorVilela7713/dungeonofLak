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

# ⚔️ Fase 2 — Player e Combate (P0)

## Tarefa 4: Movimento
- Input movimento
- Movimento 2D
- Colisão

## Tarefa 5: Vida
- PlayerHealth
- Receber dano

## Tarefa 6: Combate espada
- WeaponController
- Hitbox
- Cooldown

## Tarefa 7: Enemy básico
- Seguir player
- Atacar
- Receber dano

---

# 🧱 Fase 3 — Loop (P0)

## Tarefa 8: Sala
- RoomController
- Spawn inimigos
- Porta trava

## Tarefa 9: Loop
- Combate
- Morte
- Restart

---

# 🏛️ Fase 4 — Hub (P1)

## Tarefa 10: Mixhull
- Cena hub
- Entrada dungeon

## Tarefa 11: NPC
- Interação
- Diálogo

## Tarefa 12: Save
- Persistência de progresso

---

# 🗺️ Fase 5 — Dungeon (P1)

## Tarefa 13: Andares
- FloorManager

## Tarefa 14: Salas
- Combate + vazia

## Tarefa 15: Dificuldade
- Escala por andar

---

# 🪓 Fase 6 — Armas (P1)

## Tarefa 16: Sistema
- WeaponData
- Modular

## Tarefa 17: Lança

## Tarefa 18: Machado

---

# 👾 Fase 7 — Inimigos (P1)

## Tarefa 19
- Rápido
- Pesado
- Ranged

---

# 🎁 Fase 8 — Recompensas (P2)

## Tarefa 20: Reward

## Tarefa 21: Upgrades

---

# 🧙 Fase 9 — NPCs (P2)

## Tarefa 22
- Loja ou ferreiro

---

# 🎨 Fase 10 — Arte (P2)

## Tarefa 23: Player

## Tarefa 24: Enemy

---

# 🔊 Fase 11 — UI/Áudio (P2)

## Tarefa 25: UI

## Tarefa 26: Áudio

---

# 🛠️ Fase 12 — Polimento (P3)

## Tarefa 27: Game feel

## Tarefa 28: Balanceamento

---

# 📦 Fase 13 — Demo

## Tarefa 29: Vertical slice
- Hub funcional
- 3 andares
- 2 armas
