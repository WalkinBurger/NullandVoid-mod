using Microsoft.Xna.Framework;
using Terraria;

namespace NullandVoid.Core
{
	public struct VerletVertex(Vector2 position)
	{
		public Vector2 Position = position;
		public Vector2 OldPosition = position;
	}
	
	public class Verlet
	{
		public VerletVertex[] Vertices;
		private float vertexLength;

		public Verlet(int verticesCount, Vector2 startPosition, Vector2 endPosition, float? vertexLength = null) {
			Vertices = new VerletVertex[verticesCount];
			this.vertexLength = vertexLength ?? startPosition.Distance(endPosition) / verticesCount;

			for (int i = 0; i < verticesCount; i++) {
				Vertices[i] = new VerletVertex((endPosition - startPosition) * ((float)i / (verticesCount - 1)) + startPosition);
			}
		}

		private void UpdateVertex() {
			for (int i = 1; i < Vertices.Length - 1; i++) {
				VerletVertex vertex = Vertices[i];
				Vector2 vel = (vertex.Position - vertex.OldPosition + new Vector2(0, 1));
				vertex.OldPosition = vertex.Position;
				vertex.Position += vel;
				
				Vertices[i] = vertex;
			}
		}

		private void ApplyConstraints(Vector2 startPosition, Vector2 endPosition) {
			for (int i = 0; i < Vertices.Length - 1; i++) {
				VerletVertex vertex1 = Vertices[i];
				VerletVertex vertex2 = Vertices[i + 1];
				
				Vector2 translate = (vertex1.Position - vertex2.Position).SafeNormalize(Vector2.Zero) * (vertex1.Position.Distance(vertex2.Position) - vertexLength);

				if (i != 0 && i != Vertices.Length - 2) {
					vertex1.Position -= translate / 2;
					vertex2.Position += translate / 2;
				}
				else if (i == 0) {
					vertex1.Position = startPosition;
					vertex2.Position += translate;
				}
				else  {
					vertex2.Position = endPosition;
					vertex1.Position -= translate;
				}
				
				Vertices[i] = vertex1;
				Vertices[i + 1] = vertex2;
			}
		}

		public void Update(Vector2 startPosition, Vector2 endPosition) {
			UpdateVertex();
			for (int i = 0; i < 5; i++) {
				ApplyConstraints(startPosition, endPosition);
			}
		}
	}
}