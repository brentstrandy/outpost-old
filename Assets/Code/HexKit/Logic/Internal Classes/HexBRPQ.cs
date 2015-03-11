using System.Collections;
using System.Collections.Generic;

// This is the Blue Raja Priority Queue, heavily modified.
// See https://bitbucket.org/BlueRaja/high-speed-priority-queue-for-c/ for original.
namespace Settworks.Hexagons {
	class HexBRPQ {
		private HexBRPQNode[] _nodes = new HexBRPQNode[4];
		private ulong _numNodesEverEnqueued;
		private uint _numNodes;
		public uint Count { get { return _numNodes; } }

		public HexBRPQ() { }

		public bool Contains(HexBRPQNode node) {
			return (_nodes.Length > node.QueueIndex && _nodes[node.QueueIndex] == node);
		}

		public void Enqueue(HexBRPQNode node) {
			while (_numNodes >= _nodes.Length)
				IncreaseCapacity();
			_nodes[_numNodes] = node;
			node.Queue = this;
			node.QueueIndex = _numNodes;
			node.InsertionIndex = _numNodesEverEnqueued++;
			CascadeUp(_nodes[_numNodes]);
			_numNodes++;
		}

		void IncreaseCapacity() {
			HexBRPQNode[] newNodes = new HexBRPQNode[_nodes.Length * 2];
			for (uint i = 0; i < _nodes.Length; i++)
				newNodes[i] = _nodes[i];
			_nodes = newNodes;
		}
		
		private void Swap(HexBRPQNode node1, HexBRPQNode node2) {
			//Swap the nodes
			_nodes[node1.QueueIndex] = node2;
			_nodes[node2.QueueIndex] = node1;
			
			//Swap their indicies
			node1.QueueIndex = node1.QueueIndex ^ node2.QueueIndex;
			node2.QueueIndex = node1.QueueIndex ^ node2.QueueIndex;
			node1.QueueIndex = node1.QueueIndex ^ node2.QueueIndex;
		}
		
		private void CascadeUp(HexBRPQNode node) {
			//aka Heapify-up
			while(node.QueueIndex > 0) {
				if(HasHigherPriority(_nodes[node.ParentIndex], node))
					break;
				
				//Node has lower priority value, so move it up the heap
				Swap(node, _nodes[node.ParentIndex]); //For some reason, this is faster with Swap() rather than (less..?) individual operations, like in CascadeDown()
			}
		}
		
		private void CascadeDown(HexBRPQNode node) {
			//aka Heapify-down
			HexBRPQNode newParent;
			uint finalQueueIndex = node.QueueIndex;
			while(true) {
				newParent = node;
				uint childLeftIndex = (uint)((finalQueueIndex<<1) + 1);
				
				//Check if the left-child is higher-priority than the current node
				if(childLeftIndex >= _numNodes) {
					//This could be placed outside the loop, but then we'd have to check newParent != node twice
					node.QueueIndex = finalQueueIndex;
					_nodes[finalQueueIndex] = node;
					break;
				}
				
				HexBRPQNode childLeft = _nodes[childLeftIndex];
				if(HasHigherPriority(childLeft, newParent))
					newParent = childLeft;

				//Check if the right-child is higher-priority than either the current node or the left child
				uint childRightIndex = (uint)(childLeftIndex + 1);
				if(childRightIndex < _numNodes) {
					HexBRPQNode childRight = _nodes[childRightIndex];
					if(HasHigherPriority(childRight, newParent))
						newParent = childRight;
				}
				
				//If either of the children has higher (smaller) priority, swap and continue cascading
				if(newParent != node) {
					//Move new parent to its new index.  node will be moved once, at the end
					//Doing it this way is one less assignment operation than calling Swap()
					_nodes[finalQueueIndex] = newParent;
					
					newParent.QueueIndex = newParent.QueueIndex ^ finalQueueIndex;
					finalQueueIndex = newParent.QueueIndex ^ finalQueueIndex;
					newParent.QueueIndex = newParent.QueueIndex ^ finalQueueIndex;
				}
				else {
					//See note above
					node.QueueIndex = finalQueueIndex;
					_nodes[finalQueueIndex] = node;
					break;
				}
			}
		}
		
		private bool HasHigherPriority(HexBRPQNode higher, HexBRPQNode lower) {
			return higher.Priority < lower.Priority
			       || (higher.Priority == lower.Priority && higher.InsertionIndex < lower.InsertionIndex);
		}
		
		public HexBRPQNode Dequeue() {
			HexBRPQNode returnMe = _nodes[0];
			Remove(returnMe);
			return returnMe;
		}

		public void OnNodeUpdated(HexBRPQNode node)
		{
			//Bubble the updated node up or down as appropriate
			if(node.ParentIndex > 0 && HasHigherPriority(node, _nodes[node.ParentIndex]))
				CascadeUp(node);
			else
				//Note that CascadeDown will be called if parentNode == node (that is, node is the root)
				CascadeDown(node);
		}
		
		public void Remove(HexBRPQNode node)
		{
			if(_numNodes <= 1) {
				_nodes[0] = null;
				_numNodes = 0;
				return;
			}
			
			_numNodes--;
			//Make sure the node is the last node in the queue
			bool wasSwapped = false;
			HexBRPQNode formerLastNode = _nodes[_numNodes];
			if(node.QueueIndex != _numNodes) {
				//Swap the node with the last node
				Swap(node, formerLastNode);
				wasSwapped = true;
			}
			
			_nodes[node.QueueIndex] = null;
			
			if(wasSwapped)
				//Now bubble formerLastNode (which is no longer the last node) up or down as appropriate
				OnNodeUpdated(formerLastNode);
		}
		
	}
}