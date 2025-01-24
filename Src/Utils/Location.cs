using System;

namespace Marionette.Utils {
  public record Position(int row, int col) {
    public static Position operator +(Position pos, int offset) => new Position(pos.row, pos.col + offset);
    public static Position operator *(Position pos, int offset) => new Position(pos.row + offset, pos.col);
    public static Position operator &(Position pos, int col) => new Position(pos.row, col);
    public static Position operator |(Position pos, int row) => new Position(row, pos.col);
    public static Location operator -(Position end, Position start) => new Location(start, end);
  }

  public record Location(Position start, Position end) {
    public static bool operator &(Location loc, Position p) =>
      (p.row > loc.start.row || (p.row == loc.start.row && p.col >= loc.start.col)) &&
      (p.row < loc.end.row || (p.row == loc.end.row && p.col <= loc.end.col));
  }
} // namespace Marionette.Utils
