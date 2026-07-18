// app/src/main/java/com/yourapp/recipes/data/local/dao/MealPlanDao.kt

package com.yourapp.recipes.data.local.dao

import androidx.room.*
import com.yourapp.recipes.data.local.database.entity.MealPlanEntity
import com.yourapp.recipes.data.local.database.entity.RecipeEntity
import kotlinx.coroutines.flow.Flow

data class MealPlanWithRecipe(
    @Embedded val mealPlan: MealPlanEntity,
    @Relation(
        parentColumn = "recipe_id",
        entityColumn = "id"
    )
    val recipe: RecipeEntity
)

@Dao
interface MealPlanDao {
    
    @Query("""
        SELECT * FROM meal_plans 
        WHERE date >= :startDate AND date < :endDate
        ORDER BY date ASC
    """)
    fun getMealPlansForPeriod(startDate: Long, endDate: Long): Flow<List<MealPlanWithRecipe>>
    
    @Query("SELECT * FROM meal_plans WHERE date = :date")
    fun getMealPlansForDate(date: Long): Flow<List<MealPlanEntity>>
    
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertMealPlan(mealPlan: MealPlanEntity): Long
    
    @Update
    suspend fun updateMealPlan(mealPlan: MealPlanEntity)
    
    @Delete
    suspend fun deleteMealPlan(mealPlan: MealPlanEntity)
    
    @Query("DELETE FROM meal_plans WHERE id = :mealPlanId")
    suspend fun deleteMealPlanById(mealPlanId: Long)
    
    @Query("""
        SELECT DISTINCT r.* FROM meal_plans mp 
        INNER JOIN recipes r ON mp.recipe_id = r.id 
        WHERE mp.date >= :startDate AND mp.date < :endDate
    """)
    suspend fun getRecipesForPeriod(startDate: Long, endDate: Long): List<RecipeEntity>
}
