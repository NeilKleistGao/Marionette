using System;

namespace Marionette.Utils {
  public enum ErrorType {
    ParseError,
    CodegenError
  }

  public record Diagnosis(ErrorType type, string message, Location location);
}
