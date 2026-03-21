# Plano de Prioridade de Implementação: Roguelike da Dungeon de Mixhull

## Nome do herói
**Aeson**
- remete ao universo grego/argonauta
- soa como nome de herói
- é curto, forte e fácil de lembrar

---

## 🚀 Fase 1: Fundação Jogável e Arquitetura Base (Prioridade P0)
*O objetivo aqui é construir o esqueleto sólido do jogo: loop central, projeto limpo e estrutura que permita crescer sem retrabalho.*

1. **Setup Limpo do Projeto e Base Técnica**
   - Criar o projeto na **Unity 2D** com organização rígida de pastas: `Scenes`, `Scripts`, `Prefabs`, `Animations`, `Art`, `Audio`, `UI`, `Data`.
   - Configurar desde o início importação correta para pixel art:
     - `Filter Mode = Point`
     - `Compression = None`
     - `Pixels Per Unit` padronizado
     - `Orthographic Camera`
     - `Pixel Perfect Camera`
   - Definir resolução base do projeto para preservar identidade visual e evitar blur no upscale.
   - Criar convenções obrigatórias de nome e responsabilidades dos scripts para não virar um projeto acoplado e confuso.

2. **Core Loop Primário do Jogo**
   - O jogo precisa nascer com o loop principal já pensado:
     - cidade de **Mixhull** como hub
     - entrada na dungeon
     - progressão andar por andar
     - combate
     - morte
     - retorno/reinício
   - Antes de sistemas complexos, garantir que o fluxo mínimo exista do começo ao fim, ainda que com placeholders.

3. **Core Domain do Gameplay**
   - Estruturar as entidades centrais:
     - `Player`
     - `Weapon`
     - `Enemy`
     - `Floor`
     - `Room`
     - `NPC`
     - `Run`
     - `HubState`
   - Separar claramente o que é estado persistente do jogador e o que é estado temporário da run.
   - Desde cedo, evitar que upgrades temporários e desbloqueios permanentes fiquem misturados no mesmo fluxo.

4. **Sistema Base de Persistência**
   - Implementar save simples para:
     - armas desbloqueadas
     - NPCs liberados em Mixhull
     - progresso macro do jogador
   - Não salvar estados excessivos de combate cedo; foco em persistência segura do meta loop.
   - O save precisa ser simples, confiável e de fácil expansão.

---

## ⚔️ Fase 2: Movimento, Combate e Sensação de Controle (Prioridade P0)
*Antes de o jogo ser grande, ele precisa ser gostoso de controlar. Roguelike sem game feel forte morre cedo.*

1. **Movimentação do Aeson**
   - Criar o `PlayerController` com movimentação firme, responsiva e fácil de ajustar.
   - Definir desde o início:
     - velocidade base
     - aceleração/desaceleração, se houver
     - colisão com cenário
     - direção de olhar
   - O personagem precisa ser prazeroso de mover antes mesmo de ter arte final.

2. **Combate Base com Espada**
   - Aeson começa com espada, então esta deve ser a primeira arma totalmente funcional.
   - Implementar:
     - ataque básico
     - hitbox temporária
     - tempo de recovery
     - dano
     - feedback visual mínimo de hit
   - A espada será a referência para os futuros padrões de arma.

3. **Vida, Dano e Morte**
   - Sistema base de `Health` para player e inimigos.
   - Implementar:
     - invulnerabilidade curta após receber dano
     - morte do player
     - morte do inimigo
     - respawn ou retorno ao hub
   - O jogo precisa fechar o ciclo falha → aprendizado → nova tentativa.

4. **Game Feel Obrigatório**
   - Mesmo com placeholder, incluir:
     - knockback leve
     - flash de dano
     - freeze frame curto opcional
     - partículas simples
     - shake mínimo de câmera
   - Esses detalhes valem mais que dezenas de sistemas mal polidos.

---

## 🏛️ Fase 3: Cidade de Mixhull e Estrutura do Meta Loop (Prioridade P1)
*Mixhull não é só menu bonito. É o espaço que sustenta a progressão, a narrativa e o senso de retorno.*

1. **Cena Base do Hub**
   - Criar a cidade de Mixhull como área navegável simples.
   - Elementos mínimos:
     - ponto de spawn
     - entrada da dungeon
     - áreas reservadas para NPCs futuros
   - No começo, o hub pode ser pequeno; o importante é já existir funcionalmente.

2. **Sistema de NPCs Desbloqueáveis**
   - Criar estrutura para NPCs aparecerem com base em progresso.
   - Cada NPC deve ter:
     - condição de desbloqueio
     - presença no hub
     - diálogo base
     - função sistêmica futura
   - No início, mesmo que só um NPC seja funcional, a arquitetura precisa suportar vários.

3. **Meta Progression**
   - Definir claramente o que persiste entre runs:
     - armas liberadas
     - NPCs encontrados/liberados
     - talvez upgrades permanentes futuros
   - O jogador deve sentir que falhar numa run ainda gera avanço do ecossistema do jogo.

4. **Integração Narrativa Mínima**
   - Adicionar diálogos simples que sustentem:
     - quem é Aeson
     - o que é a dungeon
     - o papel de Mixhull
     - por que os andares importam
   - Não precisa escrever o mundo todo cedo, mas o jogo já deve respirar identidade.

---

## 🗺️ Fase 4: Estrutura da Dungeon e Progressão por Andares (Prioridade P1)
*Aqui o jogo começa a virar roguelike de verdade.*

1. **Sistema de Andares**
   - Implementar a progressão macro da dungeon de **15 andares**.
   - Inicialmente, não precisa haver 15 andares únicos completos; basta a estrutura suportar esse encadeamento.
   - Cada andar deve saber:
     - número
     - conjunto de salas
     - dificuldade relativa
     - possíveis recompensas

2. **Salas e Fluxo Interno**
   - Criar tipos básicos de sala:
     - combate
     - descanso ou recompensa
     - transição
   - Sistema mínimo de portas:
     - fecha ao iniciar combate
     - abre ao limpar inimigos
   - O fluxo por sala precisa ser claro e sem fricção.

3. **Spawn de Inimigos**
   - Criar `EnemySpawner` com parâmetros por sala.
   - Começar com poucos inimigos, mas com arquitetura que aceite:
     - waves
     - combinações
     - raridade
     - variação por andar

4. **Escalonamento de Dificuldade**
   - Não deixar dificuldade fixa.
   - O jogo precisa escalar por:
     - vida dos inimigos
     - dano
     - densidade
     - composição de encontros
   - Melhor subir complexidade tática aos poucos do que só inflar números.

---

## 🪓 Fase 5: Arsenal do Herói e Variedade de Build (Prioridade P1)
*Roguelike vive de repetição com diferença. As armas são um dos maiores motores dessa diferença.*

1. **Sistema de Armas Modular**
   - Criar `WeaponData` e `WeaponController` para que espada, lança e machado não sejam implementados como casos isolados e engessados.
   - Cada arma deve possuir:
     - alcance
     - velocidade
     - dano
     - padrão de ataque
     - cooldown
     - animação associada

2. **Espada como Template**
   - A espada é a arma inicial e deve servir como base de arquitetura.
   - Após estabilizar a espada, expandir para:
     - **lança** com maior alcance e ataque mais linear
     - **machado** com ataque mais pesado, mais lento e impacto maior

3. **Desbloqueio de Armas**
   - Integrar armas ao meta loop.
   - Elas podem ser desbloqueadas por:
     - progresso em andares
     - encontro com NPCs
     - eventos específicos da dungeon
   - O importante é que liberar nova arma mude o estilo de jogo, não apenas os números.

4. **Preparação para Builds Futuras**
   - Mesmo que no começo haja poucas armas, já deixar caminho para:
     - modificadores
     - raridades
     - efeitos especiais
     - sinergias futuras

---

## 👾 Fase 6: Inimigos, Comportamentos e Leitura de Combate (Prioridade P1)
*Sem inimigo bom, não existe roguelike bom. O jogador precisa aprender padrões, não só bater em sacos de HP.*

1. **Arquitetura Base de IA**
   - Criar base comum de inimigo:
     - patrulha ou idle
     - detecção do jogador
     - perseguição
     - ataque
     - dano
     - morte
   - Evitar scripts únicos totalmente desconectados por inimigo.

2. **Primeiros Arquétipos**
   - Implementar ao menos 3 papéis iniciais:
     - melee rápido
     - brute lento/pesado
     - ranged simples
   - Isso já cria variedade suficiente para testes bons de sala.

3. **Telegraph e Legibilidade**
   - Todo ataque importante deve ter leitura.
   - O jogador precisa perder por erro ou ganância, não por informação invisível.
   - Isso vale mais que quantidade de ataques.

4. **Sinergia entre Inimigos**
   - Conforme os andares avançam, começar a combinar inimigos com funções complementares.
   - Dificuldade boa em roguelike vem muito de composição, não só de atributos.

---

## 🎁 Fase 7: Recompensas, Progressão de Run e Escolhas (Prioridade P2)
*Aqui começa a camada viciante da run.*

1. **Sistema de Recompensas por Sala/Andar**
   - Após combates ou eventos, permitir escolhas simples:
     - cura
     - buff temporário
     - moeda
     - melhoria de arma
   - O jogo precisa recompensar o risco.

2. **Upgrades Temporários**
   - Criar sistema de bônus de run:
     - mais dano
     - mais vida
     - mais velocidade
     - efeitos em arma
   - Esses upgrades não devem persistir fora da run, para preservar identidade roguelike.

3. **Economia Básica**
   - Definir uma moeda interna da run e, se desejar, uma moeda meta separada.
   - Nunca misturar as duas sem intenção clara.

4. **Escolha Significativa**
   - A recompensa ideal não é “pegar tudo”, é “escolher uma linha”.
   - O design deve induzir trade-offs aos poucos.

---

## 🧙 Fase 8: NPCs Funcionais, Loja, Forja e Expansão do Hub (Prioridade P2)
*Mixhull precisa evoluir junto com o jogador.*

1. **Primeiros NPCs com Função**
   - Sugestão de papéis:
     - ferreiro
     - mercador
     - curandeiro
     - cronista/historiador da dungeon
   - Eles devem ser liberados gradualmente e dar sensação de reconstrução do hub.

2. **Serviços Persistentes**
   - Cada NPC deve oferecer algo claro:
     - liberar armas
     - melhorar equipamentos
     - vender itens
     - contar fragmentos da lore
   - Mesmo que inicialmente simples, o sistema deve sustentar expansão futura.

3. **Crescimento Visual de Mixhull**
   - O hub deve mudar com o progresso:
     - novos NPCs aparecem
     - áreas antes vazias ganham vida
     - objetos e decoração evoluem
   - Isso ajuda muito o jogador a sentir avanço real.

---

## 🎨 Fase 9: Arte, Pixel Art Pipeline e Identidade Visual (Prioridade P2)
*Arte não vem antes do jogo funcionar, mas também não deve entrar tarde demais a ponto de virar enxerto feio.*

1. **Pipeline de Arte**
   - Trabalhar com **LibreSprite** para sprites e animações.
   - Criar padronização de:
     - tamanho de sprite
     - paleta
     - nomenclatura
     - exportação
   - Separar placeholders de arte final para não contaminar o pipeline.

2. **Prioridade de Produção Artística**
   - Ordem ideal:
     - player
     - espada
     - inimigos básicos
     - tileset do hub
     - tileset da dungeon
     - UI
     - VFX
   - Não começar pelo cenário inteiro antes de validar o combate.

3. **Animações Essenciais**
   - Player:
     - idle
     - run
     - attack
     - hurt
     - death
   - Inimigos:
     - idle/move
     - attack
     - death

4. **Identidade Própria**
   - Inspirar-se em jogos como Dead Cells no nível de fluidez e impacto, não na cópia visual.
   - Seu jogo precisa construir sua própria linguagem de cor, ritmo e atmosfera.

---

## 🔊 Fase 10: UI, Áudio e Polimento de Feedback (Prioridade P2)
*O jogador sente qualidade antes de saber explicá-la.*

1. **UI Essencial**
   - Implementar:
     - barra de vida
     - arma atual
     - andar atual
     - tela de morte
     - feedbacks de desbloqueio
   - UI deve ser simples, legível e temática.

2. **Áudio Base**
   - Adicionar:
     - hit
     - swing
     - morte inimiga
     - dano recebido
     - música do hub
     - música da dungeon
   - Mesmo áudio provisório já aumenta muito a sensação de jogo.

3. **Polimento**
   - Ajustes em:
     - timing de ataque
     - impactos
     - transições
     - sensação de progressão
   - Polimento não é perfumaria; ele define se o jogo parece amador ou coeso.

---

## 🛠️ Fase 11: Estrutura de Conteúdo, Balanceamento e Expansão dos 15 Andares (Prioridade P3)
*Depois da fundação pronta, entra o volume de conteúdo real.*

1. **Distribuição dos 15 Andares**
   - Dividir a dungeon em blocos temáticos, por exemplo:
     - andares iniciais de introdução
     - andares centrais de complexidade
     - andares finais de alta pressão
   - Cada bloco pode introduzir novas combinações de inimigos, ambientação e ritmo.

2. **Mini-bosses / Bosses**
   - Inserir pontos marcantes entre grupos de andares.
   - Eles ajudam a quebrar repetição e criar memória forte da progressão.

3. **Balanceamento**
   - Refinar:
     - dano
     - vida
     - recompensa
     - duração da run
     - utilidade de cada arma
   - Balanceamento deve vir após o jogo estar funcional, não antes.

---

## 📦 Fase 12: Estabilidade, Save Seguro e Preparação para Demo (Prioridade P3)
*Chega a hora de transformar protótipo em algo apresentável e confiável.*

1. **Robustez de Save/Load**
   - Garantir que:
     - desbloqueios persistam corretamente
     - progresso de NPCs não corrompa
     - dados inválidos tenham fallback
   - Falha de save destrói confiança do jogador.

2. **QA de Fluxo Completo**
   - Validar:
     - hub → dungeon → morte → hub
     - desbloqueio de NPC
     - liberação de arma
     - progressão mínima por andares
   - O foco é estabilidade do loop inteiro, não perfeição local.

3. **Demo Vertical Slice**
   - Idealmente fechar uma demo com:
     - Mixhull funcional
     - 3 a 5 andares
     - 2 ou 3 armas
     - alguns NPCs
     - progressão básica
   - Antes de pensar em 15 andares completos, fechar uma fatia muito boa do jogo.

---

## Ordem real de implementação recomendada

1. setup do projeto
2. movimentação do Aeson
3. combate com espada
4. vida, dano e morte
5. uma sala funcional
6. inimigo básico
7. fluxo hub → dungeon → retorno
8. Mixhull inicial
9. persistência de desbloqueios
10. progressão de andares
11. lança
12. machado
13. mais inimigos
14. NPCs funcionais
15. recompensas e upgrades de run
16. arte final e polimento
17. balanceamento
18. demo vertical slice

---

## Definição base do jogo

- **Protagonista:** Aeson  
- **Cidade hub:** Mixhull  
- **Estrutura principal:** dungeon roguelike de 15 andares  
- **Arma inicial:** espada  
- **Armas desbloqueáveis:** lança e machado  
- **Meta loop:** liberar NPCs em Mixhull, expandir opções do jogador e avançar cada vez mais fundo na dungeon

---

## Recomendação estratégica

O maior erro aqui seria tentar fazer:
- 15 andares completos logo de cara
- vários NPCs já no início
- todas as armas ao mesmo tempo
- arte final antes do combate estar bom

O caminho mais seguro é:

**primeiro fechar um jogo pequeno que já prove o loop, depois expandir o conteúdo.**
