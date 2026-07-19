package com.yourapp.recipes.data.local.database.entity

import androidx.room.*

@Entity(
    tableName = "shopping_items",
    indices = [
        Index(value = ["category"]),
        Index(value = ["is_purchased"]),
        Index(value = ["purchase_date"])
    ]
)
data class ShoppingItemEntity(
    @PrimaryKey(autoGenerate = true)
    val id: Long = 0,
    
    @ColumnInfo(name = "name")
    val name: String,
    
    @ColumnInfo(name = "quantity")
    val quantity: Float,
    
    @ColumnInfo(name = "unit")
    val unit: String,
    
    @ColumnInfo(name = "category")
    val category: String = "other",
    
    @ColumnInfo(name = "is_purchased")
    val isPurchased: Boolean = false,
    
    @ColumnInfo(name = "recipe_id")
    val recipeId: Long? = null,
    
    @ColumnInfo(name = "price")
    val price: Float = 0f,
    
    @ColumnInfo(name = "purchase_date")
    val purchaseDate: Long? = null,
    
    @ColumnInfo(name = "date_added")
    val dateAdded: Long = System.currentTimeMillis()
)
