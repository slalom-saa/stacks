# Endpoint

An endpoint is a well defined unit of solution logic that is intended to be executed by:
1. An external client
2. An internal, but remote service
3. Another endpoint that is in the same process

## Creating an Endpoint
Getting to the point of implementing the endpoint, you should be provided with a 
service contract.  

For this example, we will use a very simple service contract: [**Add Product**](add-product-endpoint.md).

---
### Open the Starter Solution
The empty shopping solution can be found [here](https://github.com/slalom-saa/stacks-shopping/tree/master/Empty).
It has a basic project setup with nothing more.

---
### Add the Slalom.Stacks nuget package
In the NuGet Package Manager enter the following command:
```
Install-Package Slalom.Stacks
```
---
### Add project folders
Add the following folders: **Application/Catalog/Products/Add**.

This may initially feel like a lot of folders.  It won't as the solution builds out.  Here is what the folders are for

*Application* - This is where the application logic resides for now.  As the project continues, it may make sense to split this out into another project or solution.

*Catalog* - This is the bounded context for the product catalog.  Again, it is best to keep these in the same project until it makes sense to split.

*Products* - This represents the service.  See the design standards section for why there is no class here.

*Add* - This represents the operation or endpoint.  Everything in this folder will be composed to implement the logic.

---
### Add the command
In the **add** folder, add a class for the request named **AddProductCommand**.
```csharp
public class AddProductCommand
{
    /// <summary>
    /// Gets the name of the product to be added.
    /// </summary>
    /// <value>The name of the product to add.</value>
    [NotNull("Name must be specified.")]
    public string Name { get; }

    public AddProductCommand(string name)
    {
        this.Name = name;
    }
}
```
Let's break the command down.
```csharp
class AddProductCommand
```
Because the request changes state, it should end with "Command".
```csharp
public string Name { get; }
```
All properties should be immutable.  There should also be no fields.
```csharp
public AddProductCommand(string name)
{
    this.Name = name;
}
```
All properties should be set in the constructor.  Overrides can be used if a parameter is optional.
```csharp
/// <value>The name of the product to add.</value>
```
Fill out the value comments for the property name.  This will show up in swagger and other discovery documents.
Other comments should be added, but will not be used in service documents.
```csharp
[NotNull("Name must be specified.")]
```
Only basic validation should be used to indicate that the command was not serialized or deserialized properly. Most rules should be
external.

---
### Add the endpoint
Add a class named **AddProduct** to the same folder.
```csharp
/// <summary>
/// Adds a product to the product catalog so that a user can search for it and it can be added to a cart, purchased and/or shipped.
/// </summary>
[EndPoint("catalog/products/add", Name = "Add Product", Timeout = 5000, Version = 1)]
public class AddProduct : EndPoint<AddProductCommand, string>
{
    public override string Receive(AddProductCommand instance)
    {
        // Do something here to create the product.

        // return the ID
        return "[Added Product ID]";
    }
}
```
Now let's break this down a bit.
```csharp
class AddProduct
```
This should be the name of the use case.  If there are multiple versions, then append _v2 to the name for version two, etc.
```csharp
EndPoint<AddProductCommand, string>
```
The first type argument is the command and the second is the return.
```csharp
/// <summary>
/// Adds a product to the product catalog so that a user can search for it and it can be added to a cart, purchased and/or shipped.
/// </summary>
```
The summary should come directly from the service contract.  It will be used in swagger and other discovery documents.
```csharp
public override string Receive(AddProductCommand instance)
{
    // Do something here to create the product.

    // return the ID
    return "[Added Product ID]";
}
```
There are two methods that can be overridden.  The first is the synchronous method here, the other is ReceiveAsync which should be used 
when needed.