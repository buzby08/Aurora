using System.Collections;
using System.Diagnostics;

namespace Aurora.Core;

public class TokenList : IEnumerable<TokenListItem>
{
    private readonly List<TokenListItem> _data = [];

    [DebuggerDisplay("tomato")]
    public List<string> DataAsString
    {
        get
        {
            List<string> all = [];
            all.AddRange(this._data.Select(item => item.AsString));
            return all;
        }
    }

    public int Count => this._data.Count;
    public SourceLocation StartLocation => this._data.First().StartLocation;
    public SourceLocation EndLocation => this._data.Last().EndLocation;

    public void Add(Token token)
    {
        TokenListItem item = new(token);

        this._data.Add(item);
    }

    public void AddRaw(TokenListItem item)
    {
        this._data.Add(item);
    }

    public void Clear()
    {
        this._data.Clear();
    }

    public TokenListItem? FindByValue(string value)
    {
        return this._data.FirstOrDefault(item => item.AsString == value);
    }

    public TokenListItem this[int index]
    {
        get
        {
            if (index < 0 || index >= this.Count)
                throw new IndexOutOfRangeException();

            return this._data[index];
        }

        set
        {
            if (index < 0 || index >= this.Count)
                throw new IndexOutOfRangeException();

            this._data[index] = value;
        }
    }

    public TokenList this[Range range]
    {
        get
        {
            var (start, length) = range.GetOffsetAndLength(this.Count);

            TokenList slice = new();

            for (int i = start; i < start + length; i++)
            {
                slice._data.Add(this._data[i]);
            }

            return slice;
        }
    }


    public IEnumerator<TokenListItem> GetEnumerator()
    {
        return this._data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public TokenList()
    {
    }

    public TokenList(IEnumerable<TokenListItem> items)
    {
        this.Clear();
        foreach (TokenListItem item in items) this.AddRaw(item);
    }
}

public struct TokenListItem(Token token)
{
    public readonly Token Token = token;
    public SourceLocation StartLocation => token.StartLocation;
    public SourceLocation EndLocation => token.EndLocation;

    public string AsString => token.ValueAsString;

    public override string ToString() => $"TokenListItem([#{this.Token.Id}] {this.Token.Type} - {this.Token.ValueAsString})";
}
