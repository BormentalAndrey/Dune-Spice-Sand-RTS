package com.yourapp.recipes.data.local.database

import androidx.room.*
import com.yourapp.recipes.data.local.dao.*
import com.yourapp.recipes.data.local.database.entity.*
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import javax.inject.Inject
import javax.inject.Provider

@Database(
    entities = [
        RecipeEntity::class,
        IngredientEntity::class,
        MealPlanEntity::class,
        ShoppingItemEntity::class,
        RecipeCollectionEntity::class,
        RecipeCollectionCrossRef::class,
        RecipeStepEntity::class,
        UserPreferencesEntity::class
    ],
    version = 1,
    exportSchema = false
)
abstract class RecipeDatabase : RoomDatabase() {
    
    abstract fun recipeDao(): RecipeDao
    abstract fun ingredientDao(): IngredientDao
    abstract fun mealPlanDao(): MealPlanDao
    abstract fun shoppingListDao(): ShoppingListDao
    abstract fun recipeCollectionDao(): RecipeCollectionDao
    abstract fun recipeStepDao(): RecipeStepDao
    abstract fun userPreferencesDao(): UserPreferencesDao
    
    class Callback @Inject constructor(
        private val database: Provider<RecipeDatabase>
    ) : RoomDatabase.Callback() {
        
        override fun onCreate(db: androidx.sqlite.db.SupportSQLiteDatabase) {
            super.onCreate(db)
            
            CoroutineScope(Dispatchers.IO).launch {
                populateDatabase(database.get())
            }
        }
        
        private suspend fun populateDatabase(db: RecipeDatabase) {
            val popularIngredients = listOf(
                IngredientEntity(name = "Куриное филе", category = "meat", usageCount = 100),
                IngredientEntity(name = "Лук репчатый", category = "vegetables", usageCount = 95),
                IngredientEntity(name = "Морковь", category = "vegetables", usageCount = 90),
                IngredientEntity(name = "Картофель", category = "vegetables", usageCount = 85),
                IngredientEntity(name = "Чеснок", category = "vegetables", usageCount = 80)
            )
            
            popularIngredients.forEach { ingredient ->
                db.ingredientDao().insertOrUpdateIngredient(ingredient)
            }
        }
    }
    
    companion object {
        const val DATABASE_NAME = "recipes_database"
    }
}
