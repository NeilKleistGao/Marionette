using System;

namespace Marionette.Utils {
  public enum ErrorType {
    ParseError,
    RuntimeError
  }

  public record Diagnosis(ErrorType type, string message, Location location);
} // namespace Marionette.Utils
