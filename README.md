# My OpenTK Engine

Hobbyist mucking about with [the Open Toolkit](https://opentk.net/).

* **Core:** A few low-level classes that wrap around the static API classes presented by the Open Toolkit. Makes heavy use of fluent builders, like this:  
  ```
  var programBuilder = new GlProgramBuilder()
    .WithVertexShaderFromEmbeddedResource("Colored.Vertex.glsl")
    .WithFragmentShaderFromEmbeddedResource("Colored.Fragment.glsl")
    .WithDefaultUniformBlock<Uniforms>();
  
  ..

  var vertexArrayObjectBuilder = new VertexArrayObjectBuilder(PrimitiveType.Triangles)
    .WithAttributeBuffer(BufferUsageHint.StaticDraw, vertices.ToArray())
    .WithIndexBuffer(BufferUsageHint.StaticDraw, indices.ToArray());
  ```
* **ReactiveBuffers:** Building on top of core, some logic for managing buffer content via [ReactiveX](http://reactivex.io/).
* **Renderables:** Building on top of Core (and ReactiveBuffers for some of it), a simple-to-the-point-of-naivety rendering engine.

## Usage

See the [example app code](./src/ExampleApps.GameWindow) (for top-level stuff), and the various Renderables implementation projects (for usage of the classes in the Core library).
