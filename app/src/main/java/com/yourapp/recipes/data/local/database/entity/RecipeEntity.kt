package com.yourapp.recipes.data.local.database.entity

import androidx.room.*
import com.yourapp.recipes.domain.model.*

/**
 * Основная сущность рецепта в базе данных Room.
 * Содержит всю информацию о рецепте, включая ингредиенты и шаги в JSON формате.
 *
 * @property id Уникальный идентификатор рецепта
 * @property title Название рецепта
 * @property description Описание рецепта
 * @property cookingTimeMinutes Время приготовления в минутах
 * @property difficulty Сложность приготовления
 * @property category Категория блюда
 * @property ingredientsJson JSON-строка с ингредиентами
 * @property stepsJson JSON-строка с пошаговыми инструкциями
 * @property calories Калории на порцию
 * @property proteins Белки на порцию (г)
 * @property fats Жиры на порцию (г)
 * @property carbohydrates Углеводы на порцию (г)
 * @property servings Количество порций по умолчанию
 * @property photoPath Локальный путь к фото
 * @property isFavorite Избранный рецепт
 * @property dateAdded Дата добавления
 * @property dateModified Дата последнего изменения
 */
@Entity(
    tableName = "recipes",
    indices = [
        Index(value = ["title"]),
        Index(value = ["category"]),
        Index(value = ["difficulty"]),
        Index(value = ["cooking_time_minutes"]),
        Index(value = ["is_favorite"])
    ]
)
data class RecipeEntity(
    @PrimaryKey(autoGenerate = true)
    val id: Long = 0,
    
    @ColumnInfo(name = "title")
    val title: String,
    
    @ColumnInfo(name = "description")
    val description: String = "",
    
    @ColumnInfo(name = "cooking_time_minutes")
    val cookingTimeMinutes: Int = 0,
    
    @ColumnInfo(name = "difficulty")
    val difficulty: String = Difficulty.EASY.name,
    
    @ColumnInfo(name = "category")
    val category: String = Category.DINNER.name,
    
    @ColumnInfo(name = "ingredients_json")
    val ingredientsJson: String = "[]",
    
    @ColumnInfo(name = "steps_json")
    val stepsJson: String = "[]",
    
    @ColumnInfo(name = "calories")
    val calories: Float = 0f,
    
    @ColumnInfo(name = "proteins")
    val proteins: Float = 0f,
    
    @ColumnInfo(name = "fats")
    val fats: Float = 0f,
    
    @ColumnInfo(name = "carbohydrates")
    val carbohydrates: Float = 0f,
    
    @ColumnInfo(name = "servings")
    val servings: Int = 2,
    
    @ColumnInfo(name = "photo_path")
    val photoPath: String? = null,
    
    @ColumnInfo(name = "is_favorite")
    val isFavorite: Boolean = false,
    
    @ColumnInfo(name = "date_added")
    val dateAdded: Long = System.currentTimeMillis(),
    
    @ColumnInfo(name = "date_modified")
    val dateModified: Long = System.currentTimeMillis()
)
