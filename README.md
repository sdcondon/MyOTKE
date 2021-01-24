# My OpenTK Engine

A rather humble engine built on top of [the Open Toolkit](https://opentk.net/). The focus is firmly on facilitating RAD rather than performance - though effort is made to e.g. minimise GC pressure where it doesn't conflict with that (which is most of the time). Consists of:

* **Core:** Low-level classes that wrap around the static API classes presented by the Open Toolkit. Uses strong typing for OpenGL objects via generics, and makes heavy use of fluent builders, like this:  
  ```
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
* **ReactiveBuffers:** Building on top of core, some logic for managing buffer content via [ReactiveX](http://reactivex.io/).
* **Engine:** Building on top of Core (and ReactiveBuffers for some of it), a simple-to-the-point-of-naivety rendering engine.

## Usage

See the [example app code](./src/ExampleApp) for top-level stuff, and the various engine component implementation projects for usage of the classes in the Core library.
