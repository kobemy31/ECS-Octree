﻿using Unity.Collections ;
using Unity.Entities ;
using UnityEngine ;


namespace Antypodish.ECS.Octree
{


    internal class IsRayColliding_Common
    {

        static public void _DebugRays ( [ReadOnly] ref NativeArray <Entity> na_collisionChecksEntities, [ReadOnly] ref ComponentDataFromEntity <RayData> a_rayData, [ReadOnly] ref ComponentDataFromEntity <RayMaxDistanceData> a_rayMaxDistanceData, [ReadOnly] ref ComponentDataFromEntity <IsCollidingData> a_isCollidingData, [ReadOnly] ref ComponentDataFromEntity <RayEntityPair4CollisionData> a_rayEntityPair4CollisionData, bool canDebugAllChecks, bool canDebugAllrays )
        {

            // Debug all, or only one check
            int i_debugCollisionChecksCount = canDebugAllChecks ? na_collisionChecksEntities.Length : 1 ;


            // Debug
            // ! Ensure test this only with single, or at most few ray entiities.
            for ( int i_collisionChecksIndex = 0; i_collisionChecksIndex < i_debugCollisionChecksCount; i_collisionChecksIndex ++ )
            {              

                Entity octreeRayEntity = na_collisionChecksEntities [i_collisionChecksIndex] ;                
                Entity octreeRayEntity2 ;

                if ( !a_rayData.Exists ( octreeRayEntity ) )
                {
                    RayEntityPair4CollisionData rayEntityPair4CollisionData =  a_rayEntityPair4CollisionData [octreeRayEntity] ;
                    octreeRayEntity2 = rayEntityPair4CollisionData.ray2CheckEntity ;

                }
                else
                {
                    octreeRayEntity2 = octreeRayEntity ;
                }

                // Draw all available rays, or signle ray
                if ( canDebugAllrays ) 
                {
                    RayData rayData = a_rayData [octreeRayEntity2] ;
                    RayMaxDistanceData rayMaxDistanceData = a_rayMaxDistanceData [octreeRayEntity2] ;

                    Debug.DrawLine ( rayData.ray.origin, rayData.ray.origin + rayData.ray.direction * rayMaxDistanceData.f, Color.red )  ;
                }
                else if ( i_collisionChecksIndex == 0 ) 
                {                    
                    RayData rayData = a_rayData [octreeRayEntity2] ;
                    RayMaxDistanceData rayMaxDistanceData = a_rayMaxDistanceData [octreeRayEntity2] ;

                    Debug.DrawLine ( rayData.ray.origin, rayData.ray.origin + rayData.ray.direction * rayMaxDistanceData.f, Color.red )  ;
                }


                IsCollidingData isCollidingData = a_isCollidingData [octreeRayEntity] ;

                if ( isCollidingData.i_collisionsCount > 0 ) Debug.Log ( "Is colliding." ) ;                
            }

        }


        /// <summary>
	    /// Check if the specified ray intersects with anything in the tree. See also: GetColliding.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <param name="checkRay">Ray to check.</param>
	    /// <param name="f_maxDistance">Distance to check.</param>
	    /// <returns>True if there was a collision.</returns>
	    static public bool _IsNodeColliding ( [ReadOnly] ref RootNodeData rootNode, int i_nodeIndex, Ray checkRay, ref IsCollidingData isCollidingData, [ReadOnly] ref DynamicBuffer <NodeBufferElement> a_nodesBuffer, [ReadOnly] ref DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, [ReadOnly] ref DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, [ReadOnly] ref DynamicBuffer <InstanceBufferElement> a_instanceBuffer, float f_maxDistance = float.PositiveInfinity ) 
        {
		    // Is the input ray at least partially in this node?
		
            float f_distance ;

            NodeBufferElement nodeBuffer = a_nodesBuffer [i_nodeIndex] ;

		    if ( !nodeBuffer.bounds.IntersectRay ( checkRay, out f_distance ) || f_distance > f_maxDistance ) 
            {
			    return false ;
		    }
        
            if ( nodeBuffer.i_instancesCount >= 0 ) 
            {            

                int i_nodeInstancesIndexOffset = i_nodeIndex * rootNode.i_instancesAllowedCount ;

		        // Check against any objects in this node
                for (int i = 0; i < rootNode.i_instancesAllowedCount; i++) 
                {
            
                    NodeInstancesIndexBufferElement nodeInstancesIndexBuffer = a_nodeInstancesIndexBuffer [i_nodeInstancesIndexOffset + i] ;

                    // Get index of instance
                    int i_instanceIndex = nodeInstancesIndexBuffer.i ;
                
                    // Check if instance exists, and if has intersecting bounds.
                    if ( i_instanceIndex >= 0 )
                    {
                        InstanceBufferElement instanceBuffer = a_instanceBuffer [i_instanceIndex] ;

			            if ( instanceBuffer.bounds.IntersectRay ( checkRay, out f_distance) && f_distance <= f_maxDistance ) 
                        {
                            isCollidingData.i_collisionsCount = 1 ; // Is colliding
				            return true;
			            }
                    }
            
		        }
            }

            // Check children for collisions
            // Check if having children
		    if ( nodeBuffer.i_childrenCount > 0 ) 
            {

                int i_nodeChildrenIndexOffset = i_nodeIndex * 8 ;

                // We checked that is having children.
			    for (int i = 0; i < 8; i++) 
                {    
                    
                    NodeChildrenBufferElement nodeChildrenBuffer = a_nodeChildrenBuffer [i_nodeChildrenIndexOffset + i] ;
                    int i_nodeChildIndex = nodeChildrenBuffer.i_group8NodesIndex ;
                    
                    // Check if node exists
                    if ( i_nodeChildIndex >= 0 )
                    {
                        if ( _IsNodeColliding ( ref rootNode, i_nodeChildIndex, checkRay, ref isCollidingData, ref a_nodesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer, ref a_instanceBuffer, f_maxDistance ) )
                        {
                            isCollidingData.i_collisionsCount = 1 ; // Is colliding
					        return true ;
				        }
                    }
			    }
		    }

		    return false;
	    }

    }

}
