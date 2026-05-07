---
name: Approche TDD pour toute modification
description: Pour toute demande de modification de code, suivre l'ordre TDD : tests d'abord, puis code, puis vérification
type: feedback
---

Pour toute demande de modification ou nouvelle fonctionnalité, respecter cet ordre strict :

1. **Écrire les tests** en premier (cas nominaux + cas limites)
2. **Écrire le code** qui fait passer les tests
3. **Lancer les tests** et vérifier qu'ils passent tous

**Why:** L'utilisateur veut travailler en TDD pour s'assurer que le code est couvert dès le départ et que les tests guident l'implémentation.

**How to apply:** Dès qu'une modification de code est demandée, commencer par les tests avant de toucher au code de production. Ne pas sauter cette étape même si la modification semble triviale.
