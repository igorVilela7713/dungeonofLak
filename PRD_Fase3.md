# PRD — Fase 3: Loop (Sala + Morte + Restart)

## Objetivo
Implementar sistema de sala com spawn de inimigos, portas que trancam durante combate, e loop de morte/restart.

---

## Componentes Necessários

### Scripts

| Script | Responsabilidade | Pasta |
|--------|-----------------|-------|
| `RoomController` | Gerenciar sala: spawn inimigos, detectar limpeza | `Scripts/Rooms/` |
| `Door` | Porta que abre/fecha baseado no estado da sala | `Scripts/Rooms/` |
| `GameManager` | Gerenciar morte, restart, estado geral | `Scripts/Core/` |

### Prefabs

| Prefab | Descrição |
|--------|-----------|
| `Door.prefab` | Porta com Collider2D (enabled/disabled) |
| `Room.prefab` | Sala com spawn points e portas |

### Componentes Unity

- `Collider2D` (BoxCollider2D) em portas — trava passagem
- `Transform[]` spawn points — posições de spawn de inimigos
- `SceneManager.LoadScene()` — restart da cena

---

## Estrutura Sugerida

```
Assets/Scripts/
├── Core/
│   └── GameManager.cs
└── Rooms/
    ├── RoomController.cs
    └── Door.cs

Assets/Prefabs/
├── Door.prefab
└── Room.prefab
```

---

## Fluxo Básico de Funcionamento

### Loop da Sala
```
Player entra na sala (OnTriggerEnter2D)
    ↓
RoomController: fecha todas as portas
    ↓
RoomController: spawna inimigos nos spawn points
    ↓
Player luta contra inimigos
    ↓
Inimigo morre → RoomController verifica se todos mortos
    ↓
Se todos mortos → RoomController abre portas
```

### Loop de Morte/Restart
```
Player morre (PlayerHealth.OnDeath)
    ↓
GameManager: evento disparado
    ↓
SceneManager.LoadScene("Main")
    ↓
Cena reinicia do zero
```

---

## Dependências
- Usa `EnemyController` (já existe) — inimigos spawneados
- Usa `PlayerHealth.OnDeath` (já existe) — detectar morte
- Usa `IDamageable` (já existe) — inimigos são IDamageable

---

## Riscos

| Risco | Mitigação |
|-------|-----------|
| Spawn de inimigos em posição inválida | Usar spawn points fixos (Transform[]) |
| Porta fecha sem inimigos | Verificar se spawn points existem antes de fechar |
| Restart não reseta variáveis | Usar SceneManager.LoadScene para reload completo |
| Inimigos spawneados após player já saiu | Só spawnar se player estiver na sala |

---

## Notas
- Sala simples: apenas um ambiente com portas
- Spawn points colocados manualmente no editor
- Porta é apenas um GameObject com Collider2D ativado/desativado
- Restart recarrega a cena inteira (simples, sem persistência)
