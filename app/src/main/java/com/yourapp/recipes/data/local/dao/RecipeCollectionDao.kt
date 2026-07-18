package com.yourapp.recipes.data.local.dao

import androidx.room.*
import com.yourapp.recipes.data.local.database.entity.RecipeCollectionEntity
import com.yourapp.recipes.data.local.database.entity.RecipeCollectionCrossRef
import kotlinx.coroutines.flow.Flow

@Dao
interface RecipeCollectionDao {
    
    @Query("SELECT * FROM recipe_collections ORDER BY name ASC")
    fun getAllCollections(): Flow<List<RecipeCollectionEntity>>
    
    @Query("""
        SELECT r.* FROM recipes r 
        INNER JOIN recipe_collection_cross_ref cr ON r.id = cr.recipe_id 
        WHERE cr.collection_id = :collectionId
    """)
    fun getRecipesInCollection(collectionId: Long): Flow<List<RecipeEntity>>
    
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertCollection(collection: RecipeCollectionEntity): Long
    
    @Insert(onConflict = OnConflictStrategy.IGNORE)
    suspend fun addRecipeToCollection(crossRef: RecipeCollectionCrossRef)
    
    @Delete
    suspend fun removeRecipeFromCollection(crossRef: RecipeCollectionCrossRef)
    
    @Query("DELETE FROM recipe_collections WHERE id = :collectionId")
    suspend fun deleteCollection(collectionId: Long)
}
