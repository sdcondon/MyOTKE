# My OpenTK Engine

A rather humble engine built on top of [the Open Toolkit](https://opentk.net/).
The focus is firmly on facilitating RAD rather than performance - though effort is made to e.g. minimise GC pressure where it doesn't conflict with that (which is most of the time).
Consists of a number of libraries, as follows.

## MyOTKE.Core

Low-level classes that wrap around the static API classes presented by the Open Toolkit. The public API works with OpenGL objects directly (programs, shaders, buffers, etc) rather than creating its own facade, but establishes strong typing for them via generics. Makes heavy use of fluent builders. Here's an indicative example:  

```csharp
var program = new GlProgramBuilder()
    .WithVertexShaderFromEmbeddedResource("Colored.Vertex.glsl")
    .WithFragmentShaderFromEmbeddedResource("Colored.Fragment.glsl")
    .WithDefaultUniformBlock<Uniforms>()
    .Build();
  
var vertexArrayObject = new VertexArrayObjectBuilder(PrimitiveType.Triangles)
    .WithNewAttributeBuffer(BufferUsageHint.StaticDraw, vertices.ToArray())
    .WithNewIndexBuffer(BufferUsageHint.StaticDraw, indices.ToArray())
    .Build();

..

vertexArrayObject.Draw(program, new Uniforms
{
    AmbientLightColor = AmbientLightColor,
    ..
});

```

Complete usage examples can be found in the various engine component implementations (see below) - in particular, see the [basic examples](./src/Engine.Components.BasicExamples).

Remarks:
* While the fluent builder approach works well in very simple scenarios such as the above, for more complex cases where, say there are multiple UBOs used by a program, it gets a little more awkward (since, while strongly typed, your UBOs must be called UniformBufferObject1 etc). Sharing things (like buffers of various types) is also a little infleixble - you can't say, create your own buffer and share it between VAOs. In short, this library needs a bit more flexibility in letting people create their own program and VAO implementations - some lessons to be learned from ObjectTK, perhaps.

## MyOTKE.ReactiveBuffers

Building on top of MyOTKE.Core, some logic for managing buffer content via [ReactiveX](http://reactivex.io/).

Usage examples can be found in the [ReactivePrimitives](./src/Engine.Components.ReactivePrimitives) engine component implementation.
 
## MyOTKE.Engine

Building on top of Core, a simple-to-the-point-of-naivety rendering engine.

Rendering components are implemented in their own libraries:

* **MyOTKE.Engine.Components.BasicExamples:** Basic component examples
* **MyOTKE.Engine.Components.Gui:** A **really** basic (i.e. its not really good for much..) GUI component 
* **MyOTKE.Engine.ReactivePrimitives:** Renderer for primitives that are updated via ReactiveX

See the [example app code](./src/ExampleApp) for usage of the engine.
