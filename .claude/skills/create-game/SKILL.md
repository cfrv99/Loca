---
name: create-game
description: Use when implementing any game. Server-authoritative state machine with fog of war, reconnection, and SignalR integration. Covers Mafia, Truth or Dare, Uno, Domino, Quiz, Would You Rather.
---

# Create Game Skill

Read `docs/prd/feature-specs.md` Section 5 for full game specs.
Read `docs/architecture/signalr-contracts.md` GameHub for the hub contract.
Use context7: `"use context7 for SignalR .NET 8 hub real-time game state"`

## Architecture (ALL games follow this)
```
Client sends: SubmitAction(sessionId, {actionType, data})
Server: validate → apply to state → check win → broadcast player-specific state
Client receives: stateUpdated(sessionId, playerVisibleState)
```
**RULES:**
- ALL state lives on server (never trust client)
- Fog of war: each player gets ONLY their visible state (not full game state)
- Timeout: if player doesn't act → auto-action after phase deadline
- Reconnect: 60s grace, game pauses for that player, others continue
- Result: auto-post to venue public chat via VenueChatHub

## Game: Mafia (5-12 players)
**State machine:** `Lobby → RoleReveal → Night → DayDiscussion → DayVote → Elimination → (repeat Night) → GameOver`

**Roles & distribution:**
- 5p: 1 Mafia, 1 Doctor, 1 Detective, 2 Citizen
- 6-7p: 2 Mafia, 1 Doctor, 1 Detective, rest Citizen
- 8-9p: 2 Mafia, 1 Doctor, 1 Detective, rest Citizen
- 10-12p: 3 Mafia, 1 Doctor, 1 Detective, rest Citizen

**Night phase (45s):**
- Mafia votes on kill target (majority if 2+ mafia)
- Doctor chooses someone to save (can save self once)
- Detective investigates one person (learns if Mafia or not)
- Timeout: random target

**Day Discussion (120s):** text chat within game (separate from venue chat)

**Day Vote (60s):** each alive player votes to eliminate one person. Most votes = eliminated. Tie = no elimination.

**Fog of war:**
- Citizens see: alive players, elimination results, vote counts
- Mafia see: above + who other mafia are + mafia chat
- Detective sees: above + investigation results
- Dead players see everything (spectator mode)

**Win:** Mafia ≥ Citizens → Mafia wins. All Mafia dead → Citizens win.

## Game: Truth or Dare (3-10 players)
**State machine:** `Lobby → SpinnerTurn → ChoiceMade → QuestionShown → (Timer) → Complete → (repeat) → HostEnds`

**Flow per turn:**
1. Spinner animation selects random player
2. Player picks: Truth or Dare
3. System shows random question from `game.truth_or_dare_questions` (matching type + category)
4. Timer: 60s for Dare, 45s for Truth
5. Skip: costs 50 coins (deducted from wallet)
6. Custom question: any player submits → host approves/rejects
7. No winner — host presses "End Game" when done

**Categories:** Gülməli, Romantik, Ekstremal, İntellektual

## Game: Uno (2-6 players)
**State machine:** `Lobby → DealCards → PlayerTurn → (CardPlayed|DrewCard) → NextTurn → ... → UnoCall → GameOver`

**Rules:**
- 108 cards: 4 colors (Red/Yellow/Green/Blue) × (0-9, Skip, Reverse, +2) + Wild, Wild+4
- Deal 7 cards each, flip top of draw pile as starter
- Must match color OR number of top card, OR play Wild
- Special cards: Skip (next player skipped), Reverse (direction change), +2 (next draws 2 + skip), Wild (pick color), Wild+4 (pick color + next draws 4, can challenge)
- **Stacking:** +2 on +2, +4 on +4 (configurable by host)
- **UNO call:** when 1 card left, must tap "UNO!" button within 2s. If opponent catches → +2 penalty cards
- **Win:** first to 0 cards. Score = sum of opponents' remaining card values.
- Card values: number = face value, Skip/Reverse/+2 = 20, Wild/+4 = 50

## Game: Domino (2-4 players)
**State machine:** `Lobby → DealTiles → PlayerTurn → (TilePlayed|DrewFromBoneyard) → NextTurn → ... → GameOver`

**Rules:**
- 28 tiles (double-six set): [0|0] through [6|6]
- Deal: 2p=7 each, 3p=7 each (7 in boneyard), 4p=7 each
- First play: highest double, or highest total if no doubles
- Must match an open end. Can't play → draw from boneyard until can play (or boneyard empty → pass)
- Game ends: player empties hand, OR all players blocked
- Score: winner = sum of all opponents' remaining pip totals. If blocked → lowest total wins, gets difference.

## Game: Quiz Battle (2-20 players)
**State machine:** `Lobby → QuestionShown → AnswerPhase(15s) → RevealResults → (repeat ×10) → FinalScores`

**Rules:**
- 10 rounds per game
- Question from `game.quiz_questions` (random, no repeats)
- 4 answer choices, 15s timer
- Scoring: correct + fast = more points. 15s=100, 10s=80, 5s=60, <5s=40, wrong=0
- Categories: Ümumi Bilik, Azərbaycan, İdman, Musiqi, Texnologiya, Tarix
- Winner: highest total score

## Game: Would You Rather (2+ players)
**State machine:** `Lobby → QuestionShown → VotingPhase(20s) → RevealResults → (repeat) → HostEnds`

**Rules:**
- Questions from `game.would_you_rather`
- 2 options shown, all vote anonymously (20s)
- Results: percentage split + who voted what (revealed after voting closes)
- No winner, no scoring — pure engagement
- Host controls: next question, end game

## Tests for Each Game
1. Full game simulation: lobby → all phases → winner determined
2. Disconnect mid-game: player reconnects within 60s → state synced
3. Timeout: player doesn't act → auto-action applied correctly
4. Fog of war: verify each player sees ONLY their allowed data
5. Edge cases: minimum players, all disconnect, host leaves
