﻿using Unity.Collections ;
using Unity.Rendering ;
using Unity.Entities ;
using Unity.Jobs ;


namespace Antypodish.ECS.Highlight
{

    public class ResetSystem : JobComponentSystem
    {

        EndInitializationEntityCommandBufferSystem eiecb ;
        
        protected override void OnCreate ( )
        {
            // Cache the EndInitializationEntityCommandBufferSystem in a field, so we don't have to create it every frame
            eiecb = World.GetOrCreateSystem <EndInitializationEntityCommandBufferSystem> () ;            
        }


        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
                              
            UnityEngine.Debug.LogWarning ( "Reset Higlight" ) ;

            JobHandle jobHandle = new Job
            {                   
                ecb                = eiecb.CreateCommandBuffer ().ToConcurrent ()
                // renderMeshTypes    = EntityManager.GetComponentData <RenderMeshTypesData> ( Bootstrap.renderMeshTypesEntity )

            }.Schedule ( this, inputDeps ) ;

            eiecb.AddJobHandleForProducer ( jobHandle ) ;

            return jobHandle ;

        }

        /// <summary>
        /// Execute Jobs
        /// </summary>
        [RequireComponentTag ( typeof ( RenderMesh ), typeof ( ResetHighlightTag )  ) ]
        // [BurstCompile]
        struct Job : IJobForEachWithEntity <MeshTypeData>
        {
            
            public EntityCommandBuffer.Concurrent ecb ;
            
            // [ReadOnly] 
            // public RenderMeshTypesData renderMeshTypes ;

            public void Execute ( Entity highlightEntity, int jobIndex, [ReadOnly] ref MeshTypeData meshType )
            {
                
                // renderer
                RenderMesh renderMesh = Bootstrap._SelectRenderMesh ( meshType.type ) ;
                                      
                ecb.SetSharedComponent <RenderMesh> ( jobIndex, highlightEntity, renderMesh ) ; // replace renderer with material and mesh

                ecb.RemoveComponent <ResetHighlightTag> ( jobIndex, highlightEntity ) ; 
                                                   
            }           
            
        } // job

    }

}
