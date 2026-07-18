package com.yourapp.recipes.data.local.database

import androidx.room.*
import androidx.sqlite.db.SupportSQLiteDatabase
import com.yourapp.recipes.data.local.dao.*
import com.yourapp.recipes.data.local.database.entity.*
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import javax.inject.Inject
import javax.inject.Provider

/**
 * Основная база данных приложения рецептов.
 * Использует Room для локального хранения всех данных.
 * Поддерживает миграции и предзаполнение тестовыми данными.
 *
 * @property recipeDao DAO для работы с рецептами
 * @property ingredientDao DAO для работы с ингредиентами
 * @property mealPlanDao DAO для работы с планом питания
 * @property shoppingListDao DAO для работы со списком покупок
 * @property recipeCollectionDao DAO для работы с коллекциями рецептов
 */
@Database(
    entities = [
        RecipeEntity::class,
        IngredientEntity::class,
        MealPlanEntity::class,
        ShoppingItemEntity::class,
        RecipeCollectionEntity::class,
        RecipeCollectionCrossRef::class
    ],
    version = 2,
    exportSchema = true
)
@TypeConverters(Converters::class)
abstract class RecipeDatabase : RoomDatabase() {
    
    abstract fun recipeDao(): RecipeDao
    abstract fun ingredientDao(): IngredientDao
    abstract fun mealPlanDao(): MealPlanDao
    abstract fun shoppingListDao(): ShoppingListDao
    abstract fun recipeCollectionDao(): RecipeCollectionDao
    
    class Callback @Inject constructor(
        private val database: Provider<RecipeDatabase>
    ) : RoomDatabase.Callback() {
        
        override fun onCreate(db: SupportSQLiteDatabase) {
            super.onCreate(db)
            
            // Предзаполнение базы данных начальными данными
            CoroutineScope(Dispatchers.IO).launch {
                populateDatabase(database.get())
            }
        }
        
        private suspend fun populateDatabase(db: RecipeDatabase) {
            // Добавление популярных ингредиентов
            val popularIngredients = listOf(
                IngredientEntity(name = "Куриное филе", category = "meat", usageCount = 100),
                IngredientEntity(name = "Лук репчатый", category = "vegetables", usageCount = 95),
                IngredientEntity(name = "Морковь", category = "vegetables", usageCount = 90),
                IngredientEntity(name = "Картофель", category = "vegetables", usageCount = 85),
                IngredientEntity(name = "Чеснок", category = "vegetables", usageCount = 80),
                IngredientEntity(name = "Помидоры", category = "vegetables", usageCount = 75),
                IngredientEntity(name = "Мука пшеничная", category = "groceries", usageCount = 70),
                IngredientEntity(name = "Сахар", category = "groceries", usageCount = 65),
                IngredientEntity(name = "Соль", category = "spices", usageCount = 60),
                IngredientEntity(name = "Масло растительное", category = "groceries", usageCount = 55)
            )
            
            popularIngredients.forEach { ingredient ->
                db.ingredientDao().insertOrUpdateIngredient(ingredient)
            }
            
            // Создание тестовых коллекций
            val collections = listOf(
                RecipeCollectionEntity(name = "Праздничные", description = "Рецепты для праздников"),
                RecipeCollectionEntity(name = "Быстрые", description = "Блюда до 30 минут"),
                RecipeCollectionEntity(name = "Для гостей", description = "Впечатляющие блюда")
            )
            
            collections.forEach { collection ->
                db.recipeCollectionDao().insertCollection(collection)
            }
        }
    }
    
    companion object {
        const val DATABASE_NAME = "recipes_database"
    }
}

/**
 * Конвертеры типов для Room.
 * Преобразуют сложные типы данных в примитивные для хранения в SQLite.
 */
class Converters {
    
    @TypeConverter
    fun fromTimestamp(value: Long?): Date? {
        return value?.let { Date(it) }
    }
    
    @TypeConverter
    fun dateToTimestamp(date: Date?): Long? {
        return date?.time
    }
}
