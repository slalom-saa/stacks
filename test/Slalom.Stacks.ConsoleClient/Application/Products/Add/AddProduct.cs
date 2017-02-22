using System.Threading.Tasks;
using Slalom.Stacks.ConsoleClient.Application.Products.Stock;
using Slalom.Stacks.ConsoleClient.Domain.Products;
using Slalom.Stacks.Messaging;
using Slalom.Stacks.Messaging.Exceptions;

namespace Slalom.Stacks.ConsoleClient.Application.Products.Add
{
    [Path("products/add")]
    public class AddProduct : UseCase<AddProductCommand, AddProductEvent>
    {
        public override async Task<AddProductEvent> ExecuteAsync(AddProductCommand command)
        {
            var target = new Product("name");

            await this.Domain.AddAsync(target);

            var stock = await this.Send(new StockProductCommand(command.Count));
            if (!stock.IsSuccessful)
            {
                await this.Domain.RemoveAsync(target);

                throw new ChainFailedException(command, stock);
            }

            return new AddProductEvent();
        }
    }
}