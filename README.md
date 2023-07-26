## Description:

Texas Hold'em is a Poker variant in which each player is given two "hole cards". Players then proceed to make a series of bets while five "community cards" are dealt. If there are more than one player remaining when the betting stops, a showdown takes place in which players reveal their cards. Each player makes the best poker hand possible using five of the seven available cards (community cards + the player's hole cards).

Possible hands are, in descending order of value:

1. Straight-flush (five consecutive ranks of the same suit). Higher rank is better.
2. Four-of-a-kind (four cards with the same rank). Tiebreaker is first the rank, then the rank of the remaining card.
3. Full house (three cards with the same rank, two with another). Tiebreaker is first the rank of the three cards, then rank of the pair.
4. Flush (five cards of the same suit). Higher ranks are better, compared from high to low rank.
5. Straight (five consecutive ranks). Higher rank is better.
6. Three-of-a-kind (three cards of the same rank). Tiebreaker is first the rank of the three cards, then the highest other rank, then the second highest other rank.
7. Two pair (two cards of the same rank, two cards of another rank). Tiebreaker is first the rank of the high pair, then the rank of the low pair and then the rank of the remaining card.
8. Pair (two cards of the same rank). Tiebreaker is first the rank of the two cards, then the three other ranks.
9. Nothing. Tiebreaker is the rank of the cards from high to low.

Given hole cards and community cards, complete the function **hand** to return the type of hand (as written above, you can ignore case) and a list of ranks in decreasing order of significance, to use for comparison against other hands of the same type, of the best possible hand.
```C#
Hand(new[] {"A♠", "A♦"}, new[] {"J♣", "5♥", "10♥", "2♥", "3♦"})
// ...should return ("pair", new[] {"A", "J", "10", "5"})
Hand(new[] {"A♠", "K♦"}, new[] {"J♥", "5♥", "10♥", "Q♥", "3♥"})
// ...should return ("flush", new[] {"Q", "J", "10", "5", "3"})
```
**EDIT:** for Straights with an Ace, only the ace-high straight is accepted. An ace-low straight is invalid (ie. A,2,3,4,5 is invalid). This is consistent with the author's reference solution. ~docgunthrop
### My solution
```C#
using System.Collections.Generic;
using System.Linq;
using System;

public static class Kata
{
    private struct Card
    {
        public string StringRepresentation { get; private set; }
        public int Value { get; private set; }
        public char Suit { get; private set; }

        public Card(string str)
        {
            StringRepresentation = str[0..^1];
            Suit = str[^1];
            Value = StringRepresentation switch
            {
                "A" => 14,
                "K" => 13,
                "Q" => 12,
                "J" => 11,
                _ => int.Parse(StringRepresentation)
            };
        }
    }
  
    public static (string type, string[] ranks) Hand(string[] holeCards, string[] communityCards)
    {
        List<Card> cards = FillList(holeCards.Concat(communityCards).ToArray());

        if (TryToFindStraight(TryToFindFlush(cards) ?? new List<Card>() { }) != null)
            return ConvertCombinationToTuple(TryToFindStraight(TryToFindFlush(cards) ?? new List<Card>() { }), "straight-flush");

        if (TryToFindFourOfAKind(cards) != null)
            return ConvertCombinationToTuple(TryToFindFourOfAKind(cards), "four-of-a-kind");

        if (TryToFindFullHouse(new List<Card>(cards)) != null)
            return ConvertCombinationToTuple(TryToFindFullHouse(cards), "full house");

        if (TryToFindFlush(cards) != null)
            return ConvertCombinationToTuple(TryToFindFlush(cards)?.Take(5).ToList(), "flush");

        if (TryToFindStraight(cards) != null)
            return ConvertCombinationToTuple(TryToFindStraight(cards), "straight");

        if (TryToFindThreeOfAKind(new List<Card>(cards)) != null)
            return ConvertCombinationToTuple(TryToFindThreeOfAKind(cards), "three-of-a-kind");

        if (TryToFindTwoPair(new List<Card>(cards)) != null)
            return ConvertCombinationToTuple(TryToFindTwoPair(cards), "two pair");

        if (TryToFindPair(new List<Card>(cards)) != null)
            return ConvertCombinationToTuple(TryToFindPair(cards), "pair");

        return ConvertCombinationToTuple(cards.Take(5).ToList(), "nothing");
    }
  
    private static List<Card> FillList(string[] array)
    {
        List<Card> resultList = new();

        foreach (string str in array)
            resultList.Add(new Card(str));

        return resultList.OrderByDescending(x => x.Value).ToList();
    }

    private static List<Card>? TryToFindStraight(List<Card> combination)
    {
        List<Card> straightCombination = new(combination);

        for (int i = 0, count = 1; i < straightCombination.Count - 1 && count != 5; i++, count++)
        {
            if (straightCombination[i].Value - straightCombination[i + 1].Value == 0)
            {
                straightCombination.Remove(straightCombination[i + 1]);
                i -= 1;
                count -= 1;
            }

            else if (straightCombination[i].Value - straightCombination[i + 1].Value != 1)
            {
                straightCombination.RemoveRange(0, i + 1);
                i = -1;
                count = 0;
            }
        }

        return straightCombination.Count < 5 ? null : straightCombination.Take(5).ToList();
    }

    private static List<Card>? TryToFindFlush(List<Card> combination)
    {
        List<Card> flush = combination.GroupBy(x => x.Suit).Where(x => x.Count() >= 5).SelectMany(card => card).ToList();

        return flush.Count == 0 ? null : flush;
    }

    private static List<Card>? TryToFindFourOfAKind(List<Card> combination)
    {
        List<Card> fourOfAKind = combination.GroupBy(x => x.Value).Where(x => x.Count() == 4).SelectMany(card => card).ToList();

        if (fourOfAKind.Count == 0)
            return null;

        fourOfAKind.Add(combination.First(x => x.Value != fourOfAKind[0].Value));

        return fourOfAKind;
    }

    private static List<Card>? TryToFindFullHouse(List<Card> combination)
    {
        if (combination.GroupBy(x => x.Value).Count() > 4)
            return null;

        List<Card>? fullHouse = TryToDelistThree(ref combination);
        List<Card>? groupOfTwoCards = TryToDelistTwo(ref combination);

        if (fullHouse == null || groupOfTwoCards == null) 
            return null;

        fullHouse.AddRange(groupOfTwoCards);

        return fullHouse;
    }

    private static List<Card>? TryToFindThreeOfAKind(List<Card> combination)
    {
        List<Card>? threeOfAKind = TryToDelistThree(ref combination);

        if (threeOfAKind == null)
            return null;

        threeOfAKind.AddRange(combination.Take(2));

        return threeOfAKind;
    }

    private static List<Card>? TryToFindTwoPair(List<Card> combination)
    {
        if (combination.GroupBy(x => x.Value).Count() > 5)
            return null;

        List<Card>? pair1 = TryToDelistTwo(ref combination);
        List<Card>? pair2 = TryToDelistTwo(ref combination);

        if (pair1 == null || pair2 == null)
            return null;

        return new List<Card>(pair1.Concat(pair2)) { combination.First() };
    }

    private static List<Card>? TryToFindPair(List<Card> combination)
    {
        List<Card>? pair = TryToDelistTwo(ref combination);
        pair?.AddRange(combination.Take(3));

        return pair;
    }

    private static List<Card>? TryToDelistThree(ref List<Card> combination)
    {
        var groupOfThreeCards = combination.GroupBy(x => x.Value).FirstOrDefault(x => x?.Count() == 3, null);
        combination.RemoveAll(x => x.Value == groupOfThreeCards?.Key);

        return groupOfThreeCards?.ToList();
    }

    private static List<Card>? TryToDelistTwo(ref List<Card> combination)
    {
        var group = combination.GroupBy(x => x.Value).FirstOrDefault(x => x?.Count() >= 2, null);

        if (group == null)
            return null;

        List<Card> result = group.Take(2).ToList();
        combination = combination.Except(result).ToList();

        return result;
    }

    private static (string type, string[] ranks) ConvertCombinationToTuple(List<Card>? combination, string type)
    {
        if (combination == null)
            return new("", Array.Empty<string>());

        string[] ranks = combination.Select(x => x.StringRepresentation).GroupBy(x => x).OrderByDescending(x => x.Count()).Select(x => x.Key).ToArray();

        return new(type, ranks);
    }
}
```
