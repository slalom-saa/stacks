﻿using System;
using Slalom.Stacks.Search;

namespace Slalom.Stacks.Test.Search
{
    public class ItemSearchResult : ISearchResult
    {
        public int Id { get; set; }

        public string Text { get; set; }
    }
}