# My OTK Engine

Hobbyist mucking about with [the Open Toolkit](https://opentk.net/).

* **Core:** A few low-level classes that wrap around the static API classes presented by the Open Toolkit (OTK does have a wrappers lib, but they aren't really to my taste)
* **ReactiveBuffers:** Building on top of core, some logic for managing buffer content via [ReactiveX](http://reactivex.io/).
* **Views:** Building on top of Core and ReactiveBuffers, a simple-to-the-point-of-naivety rendering engine.

## Usage

See the [example app code](./src/ExampleApps.WinForms).