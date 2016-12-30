﻿using System;
using Slalom.Stacks.Search;

namespace Slalom.Stacks.Test.Examples.Search
{
    public class ItemSearchResult : ISearchResult
    {
        public int Id { get; set; }

        public string Text { get; set; }
    }
}