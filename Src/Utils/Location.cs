using System;

namespace Marionette.Utils {
  public record Position(int row, int col) {
    public static Position operator +(Position pos, int offset) => new Position(pos.row, pos.col + offset);
    public static Position operator *(Position pos, int offset) => new Position(pos.row + offset, pos.col);
    public static Location operator -(Position end, Position start) => new Location(start, end);
  }

  public record Location(Position start, Position end);
}
