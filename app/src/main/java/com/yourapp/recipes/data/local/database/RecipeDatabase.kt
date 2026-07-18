package com.yourapp.recipes.data.local.database

import androidx.room.*
import com.yourapp.recipes.data.local.dao.*
import com.yourapp.recipes.data.local.database.entity.*
import com.yourapp.recipes.data.local.RecipeJsonLoader
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
        private val database: Provider<RecipeDatabase>,
        private val jsonLoader: Provider<RecipeJsonLoader>
    ) : RoomDatabase.Callback() {
        
        override fun onCreate(db: androidx.sqlite.db.SupportSQLiteDatabase) {
            super.onCreate(db)
            
            CoroutineScope(Dispatchers.IO).launch {
                val dbInstance = database.get()
                
                // Добавляем базовые ингредиенты
                val popularIngredients = listOf(
                    IngredientEntity(name = "Куриное филе", category = "meat", usageCount = 100),
                    IngredientEntity(name = "Лук репчатый", category = "vegetables", usageCount = 95),
                    IngredientEntity(name = "Морковь", category = "vegetables", usageCount = 90),
                    IngredientEntity(name = "Картофель", category = "vegetables", usageCount = 85),
                    IngredientEntity(name = "Чеснок", category = "vegetables", usageCount = 80),
                    IngredientEntity(name = "Помидоры свежие", category = "vegetables", usageCount = 75),
                    IngredientEntity(name = "Мука пшеничная", category = "groceries", usageCount = 70),
                    IngredientEntity(name = "Сахар-песок", category = "groceries", usageCount = 65),
                    IngredientEntity(name = "Соль поваренная", category = "spices", usageCount = 60),
                    IngredientEntity(name = "Масло растительное", category = "groceries", usageCount = 55),
                    IngredientEntity(name = "Куриные яйца", category = "dairy", usageCount = 50),
                    IngredientEntity(name = "Молоко", category = "dairy", usageCount = 45),
                    IngredientEntity(name = "Сметана", category = "dairy", usageCount = 40),
                    IngredientEntity(name = "Сыр твердый", category = "dairy", usageCount = 35),
                    IngredientEntity(name = "Сливочное масло", category = "dairy", usageCount = 30),
                    IngredientEntity(name = "Рис круглозерный", category = "groceries", usageCount = 25),
                    IngredientEntity(name = "Макароны", category = "groceries", usageCount = 20),
                    IngredientEntity(name = "Гречневая крупа", category = "groceries", usageCount = 15),
                    IngredientEntity(name = "Перец черный молотый", category = "spices", usageCount = 10),
                    IngredientEntity(name = "Лавровый лист", category = "spices", usageCount = 5)
                )
                
                popularIngredients.forEach { ingredient ->
                    dbInstance.ingredientDao().insertOrUpdateIngredient(ingredient)
                }
                
                // Загружаем рецепты из JSON если их еще нет
                val recipeCount = dbInstance.recipeDao().getRecipeCount()
                if (recipeCount == 0) {
                    try {
                        val loadedCount = jsonLoader.get().loadRecipesFromAssets()
                        if (loadedCount > 0) {
                            android.util.Log.d("RecipeDatabase", "Loaded $loadedCount recipes from JSON")
                        }
                    } catch (e: Exception) {
                        android.util.Log.e("RecipeDatabase", "Error loading recipes from JSON", e)
                    }
                }
                
                // Создаем коллекции
                val collections = listOf(
                    RecipeCollectionEntity(name = "Избранное", description = "Мои любимые рецепты"),
                    RecipeCollectionEntity(name = "Праздничные", description = "Рецепты для праздников"),
                    RecipeCollectionEntity(name = "Быстрые", description = "Блюда до 30 минут"),
                    RecipeCollectionEntity(name = "Диетические", description = "Полезные и низкокалорийные блюда")
                )
                
                collections.forEach { collection ->
                    try {
                        dbInstance.recipeCollectionDao().insertCollection(collection)
                    } catch (e: Exception) {
                        android.util.Log.e("RecipeDatabase", "Error inserting collection", e)
                    }
                }
            }
        }
    }
    
    companion object {
        const val DATABASE_NAME = "recipes_database"
    }
}
