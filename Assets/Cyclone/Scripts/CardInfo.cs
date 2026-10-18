using System;

[Serializable]
public enum Suit
{
    Spade,
    Club,
    Diamond,
    Heart
}

public enum Rank
{
    Ace = 1,
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten
}

public struct CardInfo
{
    public Suit suit;
    public Rank rank;

    public CardInfo(Suit suit, Rank rank)
    {
        this.suit = suit;
        this.rank = rank;
    }

    public override string ToString()
    {
        return $"{rank} of {suit}";
    }

    public string GetSuitName(Suit suit)
    {
        return suit switch
        {
            Suit.Club => "Club",
            Suit.Diamond => "Diamond",
            Suit.Heart => "Heart",
            Suit.Spade => "Spade",
            _ => ""
        };
    }

    public string GetRankName(Rank rank)
    {
        return rank switch
        {
            Rank.Ace => "Ace",
            _ => ((int)rank).ToString()
        };
    }
}