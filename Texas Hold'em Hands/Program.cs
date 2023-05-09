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

    public static void Main()
    {
        // Some tests
        var a = Hand(new[] { "A♠", "A♦" }, new[] { "J♣", "5♥", "10♥", "2♥", "3♦" });
        // ...should return ("pair", new[] {"A", "J", "10", "5"})
        var b = Hand(new[] { "A♠", "K♦" }, new[] { "J♥", "5♥", "10♥", "Q♥", "3♥" });
        // ...should return ("flush", new[] {"Q", "J", "10", "5", "3"})
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