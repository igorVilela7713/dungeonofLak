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

# 🪓 FASE 8 — Sistema de Armas e Identidade de Combate

## 🎯 Objetivo

Implementar um sistema modular de armas que permita:

- arma inicial padrão (Sword)
- armas desbloqueáveis com estilos distintos
- troca de arma durante o jogo
- mudança visual do personagem conforme a arma equipada
- base para upgrades, raridade e sinergias futuras

---

## Prompt 8.1 — Weapon System Core

Crie o sistema central de armas do jogador.

### Contexto

- O jogo é um roguelike 2D
- O player começa com uma espada
- Novas armas serão desbloqueadas ao longo do jogo
- Cada arma deve alterar gameplay e visual
- O sistema deve ser modular, simples e preparado para expansão

### Requisitos

- Criar `WeaponData`
- Criar `WeaponController`
- Criar `WeaponType` ou enum equivalente
- Suportar arma equipada atual
- Permitir troca de arma em runtime
- Expor dados principais da arma:
  - dano
  - alcance
  - cooldown
  - knockback
  - attack direction / pattern
  - sprite placeholder da arma ou visual do player
- Integrar com o sistema de combate existente
- Preparar estrutura para upgrades futuros sem implementar tudo agora

### Critérios

- O player sempre possui uma arma equipada
- O sistema suporta troca de arma sem quebrar o combate
- Os dados de arma ficam centralizados e fáceis de ajustar
- O código não depende de if/else espalhado por todo lado

### Evitar

- Overengineering
- Criar sistema genérico demais cedo
- Misturar lógica de arma com UI ou save sem necessidade

---

## Prompt 8.2 — Sword (arma padrão)

Implemente a espada como arma inicial padrão do jogador.

### Contexto

- A sword é a arma default do jogo
- Deve servir como baseline de balanceamento
- É a arma mais equilibrada entre velocidade, dano e alcance

### Requisitos

- Criar `SwordWeaponData`
- Definir sword como arma inicial do player
- Ataque curto/médio alcance
- Velocidade de ataque média
- Dano base equilibrado
- Hitbox simples e confiável
- Integrar com visual placeholder da sword
- Atualizar sprite/visual do player ao equipar sword

### Critérios

- Player inicia o jogo com sword equipada
- A sword funciona como padrão de combate
- O visual do player muda corretamente para sword
- Ataque da sword é estável e fácil de testar

### Evitar

- Fazer a sword forte demais ou fraca demais
- Criar lógica especial excessiva para a arma padrão

---

## Prompt 8.3 — Spear

Implemente a lança.

### Contexto

- A spear deve oferecer mais alcance
- Deve incentivar combate mais seguro e linear
- Deve ter gameplay claramente diferente da sword

### Requisitos

- Criar `SpearWeaponData`
- Alcance maior que a sword
- Ataque mais linear / focado para frente
- Cooldown um pouco maior ou dano levemente diferente para compensar alcance
- Integrar com visual placeholder da spear
- Atualizar sprite/visual do player ao equipar spear

### Critérios

- A spear joga de forma diferente da sword
- O alcance maior é perceptível
- O visual do player muda corretamente para spear

### Evitar

- Fazer apenas uma sword “mais comprida”
- Dano e velocidade idênticos à sword

---

## Prompt 8.4 — Axe

Implemente o machado.

### Contexto

- O axe deve ser mais pesado
- Deve ser mais lento, porém com alto dano e impacto
- Deve incentivar timing e posicionamento

### Requisitos

- Criar `AxeWeaponData`
- Ataque mais lento
- Dano maior
- Knockback mais forte
- Alcance curto ou médio
- Integrar com visual placeholder do axe
- Atualizar sprite/visual do player ao equipar axe

### Critérios

- O axe é claramente mais lento e mais pesado
- O impacto é perceptível no combate
- O visual do player muda corretamente para axe

### Evitar

- Fazer um machado que só muda número de dano
- Deixar o cooldown tão alto que a arma fique frustrante

---

## Prompt 8.5 — Dagger (quarta arma)

Implemente uma adaga como quarta arma.

### Contexto

- A dagger adiciona um estilo oposto ao axe
- Deve ser rápida, agressiva e de curto alcance
- Boa para builds focadas em mobilidade ou DPS rápido

### Requisitos

- Criar `DaggerWeaponData`
- Ataque rápido
- Baixo ou médio dano por hit
- Alcance curto
- Cooldown baixo
- Integrar com visual placeholder da dagger
- Atualizar sprite/visual do player ao equipar dagger

### Critérios

- A dagger oferece gameplay muito mais rápida
- Diferença para sword e axe é perceptível
- O visual do player muda corretamente para dagger

### Evitar

- Fazer só uma sword com dano menor
- Deixar a arma sem identidade própria

---

## Prompt 8.6 — Weapon Visual System

Crie um sistema visual para alterar o sprite ou apresentação do player conforme a arma equipada.

### Contexto

- Por enquanto serão usados placeholders
- Mesmo sem arte final, o sistema visual já deve existir
- O player deve parecer estar com arma diferente ao trocar de equipamento

### Requisitos

- Criar um `WeaponVisualController` ou integrar isso ao controller visual do player
- Alterar sprite, child object, ou placeholder visual conforme a arma equipada
- Suportar ao menos:
  - sword
  - spear
  - axe
  - dagger
- Permitir que a troca visual aconteça automaticamente ao trocar de arma
- Não acoplar visual diretamente à lógica de dano/combate

### Critérios

- Toda troca de arma atualiza o visual do player
- O sistema funciona mesmo com placeholders
- Visual e gameplay permanecem sincronizados

### Evitar

- Hardcode excessivo no PlayerController
- Troca visual manual fora do fluxo do weapon system

---

## Prompt 8.7 — Weapon Pickup / Unlock Flow

Crie a estrutura para pickup ou unlock de armas.

### Contexto

- As armas devem ser obtidas ao longo do jogo
- O sistema deve suportar unlock permanente e/ou obtenção durante runs
- A implementação inicial pode ser simples

### Requisitos

- Criar estrutura para pickup de arma
- Permitir equipar nova arma ao interagir ou coletar
- Preparar distinção entre:
  - arma desbloqueada permanentemente
  - arma equipada na run atual
- Pode usar placeholder visual para pickup

### Critérios

- O player consegue trocar de arma via pickup ou unlock
- O sistema já suporta expansão futura para reward rooms e NPCs

### Evitar

- Implementar economia completa agora
- Misturar pickup com save permanente cedo demais

---

## Prompt 8.8 — Weapon Balancing Base

Crie uma base de balanceamento para as armas.

### Contexto

- O jogo precisa de armas com funções distintas
- O balanceamento ainda é inicial
- O importante é diferenciar papéis e feeling

### Requisitos

- Definir baseline de atributos para:
  - sword
  - spear
  - axe
  - dagger
- Garantir papéis distintos:
  - sword = equilibrada
  - spear = alcance
  - axe = dano/impacto
  - dagger = velocidade
- Centralizar os dados de tuning
- Facilitar ajustes sem alterar vários scripts

### Critérios

- Cada arma possui identidade clara
- A diferença de estilo é perceptível em gameplay
- Ajustar números é simples

### Evitar

- Balancear tudo por tentativa espalhada em múltiplos scripts
- Armas com comportamento parecido demais

---

## Prompt 8.9 — Preparação para Weapon Upgrades

Prepare o sistema para upgrades futuros de arma.

### Contexto

- Ainda não vamos implementar upgrade completo
- Mas a estrutura deve suportar evolução depois
- Isso será útil em reward rooms, bosses e NPCs

### Requisitos

- Preparar campos ou estrutura para upgrades como:
  - dano adicional
  - redução de cooldown
  - alcance extra
  - efeito elemental
  - knockback adicional
- Não precisa implementar toda a lógica agora
- Apenas garantir que o weapon system aceite extensão futura

### Critérios

- O sistema não precisa ser refeito para suportar upgrades
- Há caminho claro para expansão
- A implementação atual continua simples

### Evitar

- Implementar árvore de upgrade completa agora
- Criar sistema exageradamente abstrato

---

## Pontos importantes a considerar nesta fase

- O player deve sempre ter uma arma equipada
- A sword deve ser o baseline de referência para todas as outras armas
- Cada arma precisa mudar não só números, mas também o “feeling” do combate
- O sprite/visual do player deve mudar conforme a arma equipada, mesmo com placeholders
- A troca de arma deve ser refletida na animação, hitbox ou direção de ataque quando aplicável
- O weapon system deve ser desacoplado da UI
- O sistema deve permitir adicionar novas armas no futuro sem reescrever o player inteiro
- O sistema deve se integrar bem com reward rooms, unlocks e bosses
- Vale considerar um slot de arma atual e, futuramente, um inventário simples ou pool de armas desbloqueadas
- Algumas armas podem funcionar melhor contra certos tipos de inimigo, ajudando a criar decisões de build
- A fase já deve preparar terreno para relíquias, buffs temporários e modificadores de arma por run
- O combate deve continuar legível mesmo com armas diferentes
- A mudança visual da arma não pode depender exclusivamente da arte final; placeholders já devem funcionar

# 👾 FASE 9 — Inimigos Avançados e Ecossistema de Combate

## 🎯 Objetivo

Expandir o sistema de inimigos com:

- variedade de tipos
- elites
- bosses com identidade própria
- comportamentos distintos
- sinergia com armas e dungeon

---

## Prompt 9.1 — Enemy Archetypes (Base)

### Contexto

- O jogo precisa de variedade real de combate
- Inimigos não devem ser apenas variações de stats
- Cada tipo deve exigir resposta diferente do player

### Requisitos

Criar ao menos:

- **Melee (base)** — comportamento atual
- **Fast Enemy**
  - baixa vida
  - alta velocidade
  - pressão constante
- **Heavy Enemy**
  - alta vida
  - lento
  - alto dano / knockback
- **Ranged Enemy**
  - mantém distância
  - ataca com projéteis

### Critérios

- Cada tipo muda o comportamento do jogador
- Combate não é repetitivo
- Diferença perceptível sem olhar código

### Evitar

- Só mudar HP/dano
- IA idêntica com valores diferentes

---

## Prompt 9.2 — Elite Enemies

### Contexto

- Roguelikes usam elites como mini-desafios
- Devem aparecer ocasionalmente nas salas

### Requisitos

- Criar flag ou tipo **Elite**
- Aumentar:
  - HP
  - dano
  - tamanho ou visual (placeholder)
- Adicionar comportamento extra:
  - ataque especial
  - maior agressividade
- Chance de spawn controlada por floor

### Critérios

- Elite é claramente mais perigoso
- Player reconhece visualmente
- Gera tensão no combate

### Evitar

- Elite ser só “HP x2”
- Spawn aleatório sem controle

---

## Prompt 9.3 — Enemy Behavior System

### Contexto

- Atualmente IA é simples
- Precisamos padrões mais interessantes

### Requisitos

- Criar estados básicos:
  - idle
  - chase
  - attack
  - retreat (para ranged)
- Variar comportamento por tipo
- Adicionar:
  - tempo de reação
  - variação de movimento
  - pequenas pausas

### Critérios

- Inimigos não agem todos iguais
- Combate fica menos previsível

### Evitar

- IA complexa demais
- Overengineering

---

## Prompt 9.4 — Boss System (3 Bosses)

### Contexto

- Floors 5, 10 e 15
- Cada boss deve ser único
- Por enquanto placeholders, mas com comportamento próprio

### Requisitos

#### Boss 1 — Aggressive Melee

- perseguição constante
- ataques rápidos em sequência

#### Boss 2 — Area Control

- ataques em área
- controle de espaço
- zonas de perigo

#### Boss 3 — Hybrid / Advanced

- mistura melee + ranged
- múltiplos padrões

#### Geral

- cada boss com controller próprio
- múltiplos ataques
- integração com `BossFloorHandler`
- ao morrer:
  - player recupera vida
  - vai para reward room

### Critérios

- Boss fights são diferentes entre si
- Player precisa adaptar estratégia

### Evitar

- Reutilizar inimigo normal com stats altos
- Boss sem padrão claro

---

## Prompt 9.5 — Enemy Scaling Integration

### Requisitos

- usar `DifficultyScaler`
- escalar:
  - HP
  - dano
- elites escalam diferente
- bosses têm multiplicadores próprios

### Critérios

- dificuldade cresce de forma consistente
- não quebra balanceamento das armas

---

## Pontos importantes

- Inimigos devem complementar o sistema de armas
- Combate deve gerar decisões
- Elites e bosses aumentam variedade
- Preparar sistema para novos inimigos

---

# 🎁 FASE 10 — Recompensas e Progressão de Run

## 🎯 Objetivo

Criar o loop central de recompensa:

- escolha após combate
- upgrades temporários
- build por run

---

## Prompt 10.1 — Reward Choice System

### Contexto

- Após salas importantes ou boss
- Player escolhe 1 entre opções

### Requisitos

- apresentar 2–3 escolhas
- opções podem ser:
  - upgrade de arma
  - buff passivo
  - cura
- interface simples (placeholder)

### Critérios

- player toma decisão
- escolhas impactam a run

---

## Prompt 10.2 — Upgrade System (Run-based)

### Contexto

- Buffs duram apenas na run atual

### Requisitos

- criar estrutura de upgrade:
  - dano +
  - velocidade +
  - alcance +
  - cooldown -
- aplicar ao player ou arma
- resetar ao morrer

### Critérios

- upgrades mudam gameplay
- não persistem após morte

---

## Prompt 10.3 — Relics / Passives

### Requisitos

- buffs contínuos
- efeitos únicos:
  - +crit chance
  - +knockback
- stacking simples

---

## Prompt 10.4 — Reward Integration

### Requisitos

- integrar reward com:
  - boss
  - reward room
  - dungeon flow

---

## Pontos importantes

- Esse é o coração do roguelike
- decisões > números
- builds diferentes a cada run

---

# 🧙 FASE 11 — NPCs e Meta Progression

## 🎯 Objetivo

Criar progressão fora da run (meta).

---

## Prompt 11.1 — Shop / Blacksmith

### Requisitos

- comprar upgrades
- melhorar armas
- usar currency (runas)

---

## Prompt 11.2 — Meta Progression

### Requisitos

- upgrades permanentes:
  - HP base
  - dano base
  - unlock de armas
- salvar progresso

---

## Prompt 11.3 — NPC Unlock System

### Requisitos

- NPCs desbloqueados ao progredir
- spawn no hub

---

## Prompt 11.4 — Weapon Unlock System

### Requisitos

- armas desbloqueáveis permanentemente
- aparecem em runs futuras

---

## Pontos importantes

- separar:
  - progressão da run
  - progressão permanente

---

# 🔊 FASE 13 — UI e Áudio

## 🎯 Objetivo

Melhorar feedback e legibilidade.

---

## Prompt 13.1 — UI Core

### Requisitos

- barra de vida
- andar atual
- runas
- arma equipada
- feedback de dano (opcional)

---

## Prompt 13.2 — Combat Feedback UI

### Requisitos

- hit flash
- dano visual
- feedback ao receber dano

---

## Prompt 13.3 — Audio System

### Requisitos

- som de ataque por arma
- som de dano
- som de morte inimigo
- som de boss

---

## Prompt 13.4 — Music

### Requisitos

- música por:
  - dungeon
  - boss
  - reward room

---

## Pontos importantes

- feedback é essencial
- áudio melhora percepção de qualidade

---

# 🛠️ FASE 14 — Polimento e Game Feel

## 🎯 Objetivo

Transformar o jogo de funcional → divertido.

---

## Prompt 14.1 — Combat Feel

### Requisitos

- knockback
- hit pause
- camera shake
- particles

---

## Prompt 14.2 — Juice System

### Requisitos

- feedback visual:
  - impacto
  - morte inimigo
- diferenciar armas visualmente

---

## Prompt 14.3 — Balance Pass

### Requisitos

- ajustar:
  - armas
  - inimigos
  - bosses

---

## Prompt 14.4 — Difficulty Curve

### Requisitos

- ajustar curva por andar
- evitar spikes injustos

---

# 📦 FASE 15 — Demo / Vertical Slice

## 🎯 Objetivo

Entregar versão jogável completa.

---

## Prompt 15.1 — Vertical Slice

### Requisitos

- Hub funcional
- 5 andares
- 4 armas
- pelo menos:
  - 3 tipos de inimigos
  - 1 elite
  - 1 boss
- reward system funcionando
- UI básica

### Loop esperado

Hub → Dungeon → Combate → Reward → Boss → Reward → Morte → Hub

---

## Critérios

- gameplay loop fechado
- builds variam por run
- combate satisfatório
- sem bugs críticos

# 🚀 FASE 16 — Expansão da Dungeon e Estrutura de Exploração

## 🎯 Objetivo

Evoluir a dungeon de uma estrutura linear para uma exploração mais rica, com:

- ramificações
- verticalidade
- caminhos alternativos
- salas especiais
- progressão espacial mais interessante

---

## Prompt 16.1 — Dungeon Graph System

### Contexto

- Atualmente a dungeon é linear
- O jogo precisa de caminhos mais interessantes
- A progressão deve parecer exploração, não corredor

### Requisitos

- Evoluir o gerador para suportar um grafo de salas, não apenas linha reta
- Permitir:
  - bifurcações
  - caminhos secundários
  - dead ends com recompensa
  - rotas opcionais
- Garantir que o jogador sempre consiga chegar ao objetivo principal

### Critérios

- O layout deixa de ser apenas linear
- O jogador pode fazer escolhas de rota
- Ainda existe caminho principal garantido

### Evitar

- Algoritmo procedural complexo demais
- Mapas confusos ou injustos

---

## Prompt 16.2 — Verticality System

### Contexto

- A dungeon deve ter sensação de profundidade real
- Nem toda progressão precisa ser horizontal
- Algumas salas devem permitir descer para níveis inferiores

### Requisitos

- Adicionar suporte a salas com:
  - descida
  - plataformas
  - aberturas para baixo
  - transição para room inferior
- Permitir rooms ligadas verticalmente
- Criar estrutura para:
  - ladders
  - elevators
  - holes / drop rooms
  - stairs

### Critérios

- O mapa passa a ter progressão vertical
- Algumas salas levam para baixo em vez de apenas direita/esquerda
- O sistema ainda permanece legível

### Evitar

- Level design impossível de navegar
- Quedas injustas ou confusas

---

## Prompt 16.3 — Advanced Room Types

### Contexto

- Hoje as salas são mais focadas em combate
- A dungeon precisa de variedade funcional

### Requisitos

Criar novos tipos de sala:

- puzzle room
- trap room
- treasure room
- secret room
- elite room
- healing room
- challenge room
- event room

### Critérios

- Nem toda sala é combate padrão
- A run fica menos previsível
- Exploração ganha valor

---

## Prompt 16.4 — Descendable Rooms

### Contexto

- Algumas rooms devem permitir seguir para baixo
- Isso reforça a ideia de descida na dungeon

### Requisitos

- Criar rooms com saída inferior
- Integrar isso ao dungeon generator
- Suportar portas/escadas/queda controlada
- Atualizar o fluxo entre salas

### Critérios

- O jogador pode avançar para baixo em algumas salas
- A dungeon parece mais tridimensional mesmo em 2D

---

# 🗺️ FASE 17 — Mapa da Run e Navegação

## 🎯 Objetivo

Dar ao jogador leitura espacial da run, com mapa parcial, progressão visível e sensação de descoberta.

---

## Prompt 17.1 — Minimap / Run Map Base

### Contexto

- O jogador precisa visualizar o caminho percorrido
- O mapa pode começar bloqueado por unlock/meta progression

### Requisitos

- Criar sistema de mapa da run
- Mostrar:
  - sala atual
  - salas visitadas
  - caminho percorrido
- Ocultar salas não descobertas
- Permitir versão simplificada no início

### Critérios

- O jogador vê por onde já passou
- O mapa atualiza conforme explora

### Evitar

- Mostrar a dungeon inteira logo de cara
- UI complexa demais inicialmente

---

## Prompt 17.2 — Unlockable Map System

### Contexto

- O mapa deve ser um recurso desbloqueável
- Antes do unlock, o jogador explora mais no escuro

### Requisitos

- Sistema de unlock para habilitar o mapa
- Antes do unlock:
  - minimap escondido ou incompleto
- Depois do unlock:
  - mostra salas visitadas
  - mostra conexões conhecidas

### Critérios

- O mapa se torna parte da progressão do jogo
- O unlock faz diferença real

---

## Prompt 17.3 — Map Details

### Requisitos

- Mostrar ícones de:
  - boss
  - reward
  - elite
  - secret (se descoberto)
  - shop
- Diferenciar:
  - visitado
  - atual
  - desconhecido
  - bloqueado

### Critérios

- O mapa passa informação útil sem poluir demais

---

# 🧩 FASE 18 — Eventos, Segredos e Exploração Profunda

## 🎯 Objetivo

Adicionar surpresa, descoberta e decisões fora do combate puro.

---

## Prompt 18.1 — Secret Rooms

### Requisitos

- Criar salas secretas
- Acesso por:
  - parede falsa
  - chave
  - chance procedural
  - condição especial
- Recompensas únicas

### Critérios

- Exploração recompensa curiosidade
- Nem toda run encontra tudo

---

## Prompt 18.2 — Random Events

### Requisitos

- Criar eventos aleatórios:
  - altar
  - maldição
  - bênção
  - mercador perdido
  - escolha de sacrifício
- Eventos devem gerar decisão

### Critérios

- A run ganha momentos inesperados
- Nem tudo depende de combate

---

## Prompt 18.3 — Dungeon NPCs

### Requisitos

- NPCs temporários dentro da dungeon
- Podem oferecer:
  - lore
  - venda
  - desafio
  - cura
  - escolha arriscada

---

# 🧠 FASE 19 — Builds, Relíquias e Complexidade de Run

## 🎯 Objetivo

Fazer cada run parecer única.

---

## Prompt 19.1 — Relic System Expansion

### Requisitos

- Relíquias com efeitos mais complexos
- Sinergias com armas
- Sinergias com críticos, traps, velocidade, runes, bosses

### Critérios

- Builds passam a ter identidade real

---

## Prompt 19.2 — Weapon Modifiers

### Requisitos

- Modificadores temporários por run:
  - fire
  - poison
  - stun
  - bleed
  - lifesteal
- Aplicação em armas diferentes

---

## Prompt 19.3 — Curse / Blessing System

### Requisitos

- Buffs fortes com contrapartidas
- Ex:
  - mais dano, menos HP
  - mais runas, mais elites
  - mais velocidade, menos controle

### Critérios

- Mais risco/recompensa
- Runs mais variadas

---

# 🏛️ FASE 20 — Meta Progression Avançada

## 🎯 Objetivo

Expandir a progressão permanente sem quebrar o espírito roguelike.

---

## Prompt 20.1 — Unlock Tree

### Requisitos

- Árvore simples de unlocks
- Desbloquear:
  - armas
  - NPCs
  - relíquias
  - mapa
  - tipos de sala
  - bosses futuros

---

## Prompt 20.2 — Hub Evolution

### Requisitos

- Mixhull evolui visualmente
- NPCs aparecem
- áreas novas são abertas
- feedback visual da progressão

---

## Prompt 20.3 — Persistent Systems

### Requisitos

- salvar progresso permanente
- não salvar buffs da run
- separar claramente:
  - meta progression
  - run state

---

# 🎨 FASE 21 — Biomas, Temas e Identidade Visual da Dungeon

## 🎯 Objetivo

Quebrar repetição visual e reforçar sensação de jornada.

---

## Prompt 21.1 — Dungeon Biomes

### Requisitos

- dividir dungeon em temas
  - floors 1–4
  - floors 6–9
  - floors 11–14
- cada faixa com:
  - tiles
  - inimigos
  - traps
  - música

---

## Prompt 21.2 — Thematic Room Pools

### Requisitos

- rooms específicas por bioma
- hazards específicos por área
- bosses com identidade temática

---

# 📊 FASE 22 — Polimento Sistêmico e Replayability

## 🎯 Objetivo

Melhorar profundidade sem necessariamente adicionar muito conteúdo novo.

---

## Prompt 22.1 — Seed System

### Requisitos

- gerar dungeon por seed
- permitir reproduzir runs
- útil para debug e desafio diário

---

## Prompt 22.2 — Daily / Challenge Runs

### Requisitos

- modos especiais
- modificadores globais
- runs com regras diferentes

---

## Prompt 22.3 — Analytics / Debug Tools

### Requisitos

- visualizar:
  - seed atual
  - floor atual
  - room type
  - enemy scaling
- útil para balanceamento interno

---

# 🏁 FASE 23 — Preparação para Early Demo Pública / Playtest

## 🎯 Objetivo

Levar o jogo de “funciona na máquina do dev” para “testável por outras pessoas”.

---

## Prompt 23.1 — Playtest Build

### Requisitos

- fluxo completo estável
- tutorial mínimo
- menus básicos
- reset de save
- feedback visual claro

---

## Prompt 23.2 — New Player Experience

### Requisitos

- onboarding leve
- explicar:
  - ataque
  - interação
  - reward
  - boss
  - save
  - mapa (se desbloqueado)

---

## Prompt 23.3 — Bug Fix & Stability Pass

### Requisitos

- corrigir soft locks
- corrigir transições quebradas
- corrigir spawn inválido
- corrigir rooms sem saída

---

# Tipos de melhorias que valem muito após a Fase 15

- Transformar o layout procedural de linear para **grafo com ramificações**
- Adicionar **verticalidade** com rooms que permitem descer
- Criar **rooms com função própria**, não só combate
- Implementar **mapa da run** com progressão parcial
- Fazer o mapa ser **desbloqueável por meta progression**
- Adicionar **salas secretas**
- Colocar **eventos aleatórios** e escolhas de risco/recompensa
- Expandir **relíquias e modificadores de build**
- Criar **biomas/temas visuais** na dungeon
- Adicionar **seed de geração**
- Melhorar progressão permanente no hub
- Dar mais identidade a floors, bosses e exploração

# Ordem recomendada após a Fase 15

1. Fase 16 — Expansão da Dungeon
2. Fase 17 — Mapa da Run
3. Fase 18 — Eventos e Segredos
4. Fase 19 — Builds e Relíquias Avançadas
5. Fase 20 — Meta Progression Avançada
6. Fase 21 — Biomas e Identidade Visual
7. Fase 22 — Replayability e Ferramentas
8. Fase 23 — Playtest / Demo pública
