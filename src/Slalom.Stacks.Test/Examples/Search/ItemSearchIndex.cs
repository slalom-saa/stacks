﻿using System.Threading.Tasks;
using Slalom.Stacks.Domain;
using Slalom.Stacks.Messaging;
using Slalom.Stacks.Runtime;
using Slalom.Stacks.Search;
using Slalom.Stacks.Test.Examples.Actors.Items.Add;

namespace Slalom.Stacks.Test.Examples.Search
{
    public class ItemSearchIndexer : SearchIndexer<ItemSearchResult>, IHandleEvent<AddItemEvent>
    {
        public ItemSearchIndexer(ISearchContext context)
            : base(context)
        {
        }

        public IDomainFacade Domain { get; set; }

        public override async Task RebuildIndexAsync()
        {
        }

        public async Task HandleAsync(AddItemEvent instance, ExecutionContext context)
        {
            await this.AddAsync(new[] { new ItemSearchResult
            {
                Text = instance.Item.Text
            }});
        }
    }
}