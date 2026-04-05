using System.Collections;
using System.Diagnostics;

namespace Aurora;

internal class TokenList : IEnumerable<TokenListItem>
{
    private readonly List<TokenListItem> _data = [];

    [DebuggerDisplay("tomato")] public List<string> DataAsString
    {
        get
        {
            List<string> all = [];
            all.AddRange(this._data.Select(item => item.AsString));
            return all;
        }
    }

    private int totalChars;

    public int Count => _data.Count;

    public void Add(Token token)
    {
        TokenListItem item = new(token, tokenIndex: this.Count, startCharPosition: this.totalChars + 1);
        this.totalChars += item.EndCharPosition - item.StartCharPosition;
        this._data.Add(item);
    }

    public void AddRaw(TokenListItem item)
    {
        this.totalChars += item.EndCharPosition - item.StartCharPosition;
        this._data.Add(item);
    }

    public void Clear()
    {
        this.totalChars = 0;
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
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();

            return this._data[index];
        }

        set
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();

            this._data[index] = value;
        }
    }

    public TokenList this[Range range]
    {
        get
        {
            var (start, length) = range.GetOffsetAndLength(Count);

            TokenList slice = new();

            for (int i = start; i < start + length; i++)
            {
                slice._data.Add(_data[i]);
            }

            return slice;
        }
    }


    public IEnumerator<TokenListItem> GetEnumerator()
    {
        return _data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}

internal struct TokenListItem(Token token, int tokenIndex, int startCharPosition)
{
    public readonly Token Token = token;
    public readonly int TokenIndex = tokenIndex;
    public readonly int StartCharPosition = startCharPosition;
    public readonly int EndCharPosition = startCharPosition + token.ValueAsString.Length;

    public string AsString => token.ValueAsString;
}