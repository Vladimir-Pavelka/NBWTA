namespace Tests
{
    using System.Drawing;
    using System.Linq;
    using FluentAssertions;
    using NBWTA.Utils;
    using Xunit;

    public class LineTest
    {
        [Fact]
        public void Horizontal_Line_Slope_Should_Be_Zero()
        {
            var line = Line.FromPoints(new Point(2, 3), new Point(4, 3));
            var lineLeastSquares = Line.FromPoints(new[] { new Point(2, 3), new Point(4, 3) });

            new[] { line, lineLeastSquares }.ToList().ForEach(l =>
            {
                line.Slope.Should().Be(0);
                line.IsHorizontal.Should().BeTrue();
            });
        }

        [Fact]
        public void Directly_Proportional_Line_Should_Have_Slope_One_And_Offset_Zero()
        {
            var line = Line.FromPoints(new Point(1, 1), new Point(4, 4));
            var lineLeastSquares = Line.FromPoints(new[] { new Point(1, 1), new Point(4, 4), new Point(7, 7), });

            new[] { line, lineLeastSquares }.ToList().ForEach(l =>
            {
                line.Slope.Should().Be(1);
                line.Offset.Should().Be(0);
            });
        }

        [Fact]
        public void Perpendicular_To_Directly_Proportional_Line_Should_Have_Slope_Minus_One()
        {
            var line = Line.FromPoints(new Point(1, 1), new Point(4, 4));
            var perpendicular = line.GetPerpendicularAt(new Point(3, 3));

            perpendicular.Slope.Should().Be(-1);
            perpendicular.Offset.Should().Be(6);
        }

        [Fact]
        public void Least_Squares_Approximated_Line_Should_Be_Calculated_Correctly()
        {
            var lineLeastSquares = Line.FromPoints(new[]
                {new Point(1, 1), new Point(1, -1), new Point(7, 7), new Point(7, -7)});

            lineLeastSquares.IsHorizontal.Should().BeTrue();
            lineLeastSquares.Offset.Should().Be(0);
        }

        [Fact]
        public void Perpendicular_ToHorizontal_Should_Be_Vertical()
        {
            var horizontal = Line.FromPoints(new Point(2, 4), new Point(5, 4));
            var vertical = horizontal.GetPerpendicularAt(new Point(7, 4));
            var convertedBack = vertical.GetPerpendicularAt(new Point(7, 4));

            vertical.IsVertical.Should().BeTrue();
            convertedBack.IsHorizontal.Should().BeTrue();
            convertedBack.Offset.Should().Be(4);
        }

        [Fact]
        public void Order_Of_Points_Does_Not_Matter()
        {
            var line1 = Line.FromPoints(new Point(1, 5), new Point(5, 1));
            var line2 = Line.FromPoints(new Point(5, 1), new Point(1, 5));

            line1.Slope.Should().Be(line2.Slope);
            line1.Offset.Should().Be(line2.Offset);
        }

        [Fact]
        public void Vertical_Line_Yields_Horizontal_Indices()
        {
            var horizontalLine = Line.FromPoints(new Point(1, 5), new Point(5, 5));
            var verticalLine = Line.FromPoints(new Point(1, 5), new Point(1, 10));

            horizontalLine.LeftIndexesFrom((3, 5)).Take(3).Should()
                .BeEquivalentTo(new[] { new Point(2, 5), new Point(1, 5), new Point(0, 5) });
            horizontalLine.RightIndexesFrom((3, 5)).Take(3).Should()
                .BeEquivalentTo(new[] { new Point(4, 5), new Point(5, 5), new Point(6, 5) });

            verticalLine.LeftIndexesFrom((1, 7)).Take(3).Should()
                .BeEquivalentTo(new[] { new Point(1, 6), new Point(1, 5), new Point(1, 4) });
            verticalLine.RightIndexesFrom((1, 7)).Take(3).Should()
                .BeEquivalentTo(new[] { new Point(1, 8), new Point(1, 9), new Point(1, 10) });
        }

        [Fact]
        public void Directly_Proportional_Line_Yields_Diagonal_Indices()
        {
            var directProportion = Line.FromPoints(new Point(1, 1), new Point(3, 3));
            directProportion.LeftIndexesFrom((4, 4)).Take(7).Should().BeEquivalentTo(new[]
            {
                new Point(3, 3), new Point(3, 3), new Point(2, 2), new Point(1, 1), new Point(0, 0), new Point(0, 0), new Point(-1, -1)
            });

            directProportion.RightIndexesFrom((4, 4)).Take(7).Should().BeEquivalentTo(new[]
            {
                new Point(5, 5), new Point(5, 5), new Point(6, 6), new Point(7, 7), new Point(8, 8), new Point(8, 8), new Point(9, 9)
            });
        }
    }
}