/*
Navicat MySQL Data Transfer

Source Server         : 虚拟机
Source Server Version : 50731
Source Host           : 192.168.207.137:3306
Source Database       : bigbang

Target Server Type    : MYSQL
Target Server Version : 50731
File Encoding         : 65001

Date: 2020-09-25 09:18:55
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for addr
-- ----------------------------
DROP TABLE IF EXISTS `addr`;
CREATE TABLE `addr` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `addr` varchar(64) DEFAULT NULL,
  `is_use` bit(1) DEFAULT NULL COMMENT '是否使用',
  `master` varchar(20) DEFAULT NULL COMMENT '主人',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=5006 DEFAULT CHARSET=latin1;

-- ----------------------------
-- Table structure for Block
-- ----------------------------
DROP TABLE IF EXISTS `Block`;
CREATE TABLE `Block` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `hash` varchar(64) DEFAULT NULL,
  `fork_hash` varchar(64) DEFAULT NULL,
  `prev_hash` varchar(64) DEFAULT NULL,
  `time` bigint(20) DEFAULT NULL,
  `height` int(11) DEFAULT NULL,
  `type` varchar(16) DEFAULT NULL,
  `reward_address` varchar(64) DEFAULT NULL COMMENT '出块奖励地址',
  `reward_money` decimal(20,10) DEFAULT NULL COMMENT '出块奖励金额',
  `is_useful` bit(1) DEFAULT b'1' COMMENT '是否有效，为最长链的区块',
  `bits` int(255) DEFAULT NULL COMMENT '难度',
  `reward_state` bit(1) DEFAULT b'0' COMMENT 'DPOS出块收益计算状态',
  PRIMARY KEY (`id`) USING BTREE,
  KEY `hash` (`hash`) USING BTREE,
  KEY `height` (`height`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=282586 DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
-- Table structure for DposPayment
-- ----------------------------
DROP TABLE IF EXISTS `DposPayment`;
CREATE TABLE `DposPayment` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `dpos_addr` varchar(64) DEFAULT NULL COMMENT 'dpos 地址',
  `client_addr` varchar(64) DEFAULT NULL COMMENT '投票者收款地址',
  `payment_date` date DEFAULT NULL COMMENT '投票者收益的日期',
  `payment_money` decimal(20,10) DEFAULT NULL COMMENT '投票者在这个日期内的收益',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=32305 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Table structure for DposState
-- ----------------------------
DROP TABLE IF EXISTS `DposState`;
CREATE TABLE `DposState` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `dpos_addr` varchar(64) DEFAULT NULL COMMENT 'dpos 地址',
  `client_addr` varchar(64) DEFAULT NULL COMMENT '投票者地址',
  `audit_date` date DEFAULT NULL COMMENT '清算日期',
  `audit_money` decimal(20,10) DEFAULT NULL COMMENT '清算金额',
  PRIMARY KEY (`id`) USING BTREE,
  KEY `dpos_addr` (`dpos_addr`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=34250 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Table structure for Tx
-- ----------------------------
DROP TABLE IF EXISTS `Tx`;
CREATE TABLE `Tx` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `block_hash` varchar(64) DEFAULT NULL,
  `txid` varchar(64) DEFAULT NULL,
  `form` varchar(64) DEFAULT NULL,
  `to` varchar(64) DEFAULT NULL,
  `amount` decimal(20,10) DEFAULT NULL,
  `free` decimal(20,10) DEFAULT NULL,
  `type` varchar(16) DEFAULT NULL,
  `lock_until` int(11) DEFAULT NULL,
  `n` tinyint(6) DEFAULT NULL,
  `spend_txid` varchar(64) DEFAULT NULL,
  `data` varchar(4096) DEFAULT NULL,
  `dpos_in` varchar(64) DEFAULT NULL COMMENT '投票的dpos地址',
  `client_in` varchar(64) DEFAULT NULL COMMENT '投票的客户地址',
  `dpos_out` varchar(64) DEFAULT NULL COMMENT '赎回的dpos地址',
  `client_out` varchar(64) DEFAULT NULL COMMENT '赎回的客户地址',
  PRIMARY KEY (`id`) USING BTREE,
  KEY `block_id` (`block_hash`) USING BTREE,
  KEY `txid` (`txid`) USING BTREE,
  KEY `spend_txid` (`spend_txid`) USING BTREE,
  KEY `to` (`to`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=1095279 DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;
